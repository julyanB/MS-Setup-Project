using Refit;

namespace EmployeeManagementService.Application.ExternalClients;

public interface IBankingOperationsApi
{
    [Get("/health")]
    Task<HttpResponseMessage> HealthAsync(CancellationToken cancellationToken = default);
}
