using EmployeeManagementService.Application;
using EmployeeManagementService.Application.Common;
using EmployeeManagementService.Domain.Models;
using EmployeeManagementService.Infrastructure;
using EmployeeManagementService.Infrastructure.BackgroundJobs;
using EmployeeManagementService.Infrastructure.Configuration;
using EmployeeManagementService.Infrastructure.Identity.Authorization;
using EmployeeManagementService.Infrastructure.Persistence;
using EmployeeManagementService.Web;
using EmployeeManagementService.Web.Hubs;
using EmployeeManagementService.Web.Middleware;
using Hangfire;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Load infrastructure-specific settings from separate JSON files.
// Environment-specific overrides (kafka.Development.json, redis.Production.json, etc.)
// are picked up automatically based on ASPNETCORE_ENVIRONMENT.
builder.Configuration
    .AddJsonFile("kafka.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"kafka.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("redis.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"redis.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("hangfire.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"hangfire.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Host.UseSerilog((context, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration));

builder.Services
    .AddHttpLogging(logging =>
    {
        logging.LoggingFields = HttpLoggingFields.All;
        logging.MediaTypeOptions.AddText("application/json");
        logging.MediaTypeOptions.AddText("application/x-www-form-urlencoded");
        logging.MediaTypeOptions.AddText("multipart/form-data");
        logging.MediaTypeOptions.AddText("text/plain");
        logging.RequestBodyLogLimit = 4096;
        logging.ResponseBodyLogLimit = 4096;
        logging.RequestHeaders.Add(Headers.CorrelationId);
        logging.ResponseHeaders.Add(Headers.CorrelationId);
        logging.RequestHeaders.Add(Headers.ChannelName);
        logging.ResponseHeaders.Add(Headers.ChannelName);
        logging.ResponseHeaders.Add(Headers.ServerTime);
        logging.ResponseHeaders.Add(Headers.ProcessingDuration);
    })
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    })
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration, builder.Environment)
    .AddWebComponents();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EmployeeManagementServiceDbContext>();
    await db.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] seededRoles = ["Admin"];

    foreach (var roleName in seededRoles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    var adminRole = await roleManager.FindByNameAsync("Admin");
    if (adminRole != null)
    {
        // Ensure the "roles.manage" permission exists in the Permissions table
        var rolesManagePermission = await db.Permissions
            .FirstOrDefaultAsync(p => p.Name == PermissionClaims.RolesManage);

        if (rolesManagePermission is null)
        {
            rolesManagePermission = new Permission
            {
                Name = PermissionClaims.RolesManage,
                Description = "Manage roles and permissions",
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Permissions.Add(rolesManagePermission);
            await db.SaveChangesAsync();
        }

        // Bind the permission to the Admin role if not already bound
        var alreadyBound = await db.RolePermissions
            .AnyAsync(rp => rp.RoleId == adminRole.Id && rp.PermissionId == rolesManagePermission.Id);

        if (!alreadyBound)
        {
            db.RolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = rolesManagePermission.Id
            });
            await db.SaveChangesAsync();
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCorrelationId();
app.UseSerilogRequestLogging();
app.UseHttpLogging();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});
app.UseCustomExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (builder.Configuration.GetValue<bool>("Features:SignalR"))
{
    app.MapHub<ExampleHub>("/hubs/example");
}

var hangfireSettings = builder.Configuration
    .GetSection(HangfireSettings.SectionName)
    .Get<HangfireSettings>() ?? new HangfireSettings();

if (hangfireSettings.Dashboard.Enabled)
{
    app.UseHangfireDashboard(hangfireSettings.Dashboard.Path, new DashboardOptions
    {
        AppPath = null,
        DashboardTitle = hangfireSettings.Dashboard.AppName,
        Authorization =
        [
            new HangfireDashboardAuthorizationFilter(hangfireSettings.Dashboard.AllowAnonymous),
        ],
    });
}

app.Run();

public partial class Program { }
