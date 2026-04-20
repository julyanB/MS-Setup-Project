using Refit;

namespace EmployeeManagementService.Application.ExternalClients;

public interface ICoreServiceApi
{
    [Get("/health")]
    Task<HttpResponseMessage> HealthAsync(CancellationToken cancellationToken = default);
}
