using Refit;

namespace BankingOperationsService.Application.ExternalClients;

public interface ICoreServiceApi
{
    [Get("/health")]
    Task<HttpResponseMessage> HealthAsync(CancellationToken cancellationToken = default);
}
