using Refit;

namespace CoreService.Application.ExternalClients;

public interface IEmployeeManagementApi
{
    [Get("/health")]
    Task<HttpResponseMessage> HealthAsync(CancellationToken cancellationToken = default);
}
