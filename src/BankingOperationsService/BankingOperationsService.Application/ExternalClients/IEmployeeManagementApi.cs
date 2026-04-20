using Refit;

namespace BankingOperationsService.Application.ExternalClients;

public interface IEmployeeManagementApi
{
    [Get("/health")]
    Task<HttpResponseMessage> HealthAsync(CancellationToken cancellationToken = default);
}
