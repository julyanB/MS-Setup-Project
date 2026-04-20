using Microsoft.Extensions.Primitives;

namespace Gateway.Middleware;

public static class CorrelationIdMiddleware
{
    public const string CorrelationIdHeader = "X-Correlation-Id";

    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var existing = context.Request.Headers[CorrelationIdHeader].ToString();
            var correlationId = string.IsNullOrEmpty(existing) ? Guid.NewGuid().ToString() : existing;

            context.Request.Headers[CorrelationIdHeader] = correlationId;

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeader] = new StringValues(correlationId);
                return Task.CompletedTask;
            });

            await next();
        });
    }
}
