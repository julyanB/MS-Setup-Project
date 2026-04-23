using System.Security.Claims;
using System.Text;
using Gateway.Authorization;
using Gateway.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Host.UseSerilog((context, services, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var secret = configuration
    .GetSection("ApplicationSettings")
    .GetValue<string>("Secret")!;
var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(bearer =>
    {
        bearer.RequireHttpsMetadata = false;
        bearer.SaveToken = true;
        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("authenticated", policy =>
        policy.RequireAuthenticatedUser());
});

// Enables "permission:{name}" as a valid AuthorizationPolicy in YARP route config.
// Any route can now declare: "AuthorizationPolicy": "permission:banking.payments.read"
// without touching code — just update appsettings.json.
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(configuration.GetSection("ReverseProxy"))
    .AddTransforms(context =>
    {
        context.AddRequestTransform(transform =>
        {
            var request = transform.ProxyRequest;

            request.Headers.Remove("X-User-Id");
            request.Headers.Remove("X-User-Email");
            request.Headers.Remove("X-User-Roles");
            request.Headers.Remove("X-User-Permissions");

            var user = transform.HttpContext.User;
            if (user.Identity?.IsAuthenticated != true)
            {
                return default;
            }

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? user.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                request.Headers.Add("X-User-Id", userId);
            }

            var email = user.FindFirst(ClaimTypes.Name)?.Value
                        ?? user.FindFirst("email")?.Value;
            if (!string.IsNullOrEmpty(email))
            {
                request.Headers.Add("X-User-Email", email);
            }

            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
            if (roles.Length > 0)
            {
                request.Headers.Add("X-User-Roles", string.Join(",", roles));
            }

            var permissions = user.FindAll("permission").Select(c => c.Value).ToArray();
            if (permissions.Length > 0)
            {
                request.Headers.Add("X-User-Permissions", string.Join(",", permissions));
            }

            return default;
        });
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSerilogRequestLogging();

app.UseCorrelationId();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapHealthChecks("/ready");

app.MapReverseProxy().RequireCors();

app.Run();
