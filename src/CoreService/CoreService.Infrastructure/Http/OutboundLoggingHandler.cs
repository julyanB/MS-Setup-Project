using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CoreService.Infrastructure.Http;

public class OutboundLoggingHandler : DelegatingHandler
{
    private readonly ILogger<OutboundLoggingHandler> _logger;

    public OutboundLoggingHandler(ILogger<OutboundLoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Outbound HTTP {Method} {Url}",
            request.Method,
            request.RequestUri);

        if (request.Content is not null)
        {
            var body = await request.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Outbound request body: {Body}", body);
        }

        var stopwatch = Stopwatch.StartNew();
        var response = await base.SendAsync(request, cancellationToken);
        stopwatch.Stop();

        _logger.LogInformation(
            "Outbound HTTP {Method} {Url} responded {StatusCode} in {ElapsedMs}ms",
            request.Method,
            request.RequestUri,
            (int)response.StatusCode,
            stopwatch.ElapsedMilliseconds);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Outbound HTTP {Method} {Url} failed with {StatusCode}. Response body: {Body}",
                request.Method,
                request.RequestUri,
                (int)response.StatusCode,
                body);
        }

        return response;
    }
}
