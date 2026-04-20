using Refit;

namespace CoreService.Application.ExternalClients;

public interface IBankingOperationsApi
{
    [Get("/health")]
    Task<HttpResponseMessage> HealthAsync(CancellationToken cancellationToken = default);
}
