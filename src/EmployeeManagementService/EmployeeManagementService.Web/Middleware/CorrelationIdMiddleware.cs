using EmployeeManagementService.Application.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace EmployeeManagementService.Web.Middleware;

public static class CorrelationIdMiddleware
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var existing = context.Request.Headers[Headers.CorrelationId].ToString();
            var correlationId = string.IsNullOrEmpty(existing) ? Guid.NewGuid().ToString() : existing;

            context.Request.Headers[Headers.CorrelationId] = correlationId;

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[Headers.CorrelationId] = new StringValues(correlationId);
                return Task.CompletedTask;
            });

            await next();
        });
    }
}
