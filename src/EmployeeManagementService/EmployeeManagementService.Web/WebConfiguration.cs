using EmployeeManagementService.Application.Contracts;
using EmployeeManagementService.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;

namespace EmployeeManagementService.Web;

public static class WebConfiguration
{
    public static IServiceCollection AddWebComponents(this IServiceCollection services)
    {
        services
            .AddHttpContextAccessor()
            .AddScoped<ICurrentUser, CurrentUser>()
            .AddControllers()
            .AddApplicationPart(typeof(WebConfiguration).Assembly)
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        return services;
    }
}
