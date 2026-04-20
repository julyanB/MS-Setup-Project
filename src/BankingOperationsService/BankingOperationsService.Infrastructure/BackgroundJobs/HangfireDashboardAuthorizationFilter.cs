using BankingOperationsService.Application.Contracts;
using Hangfire.Dashboard;
using Microsoft.Extensions.DependencyInjection;

namespace BankingOperationsService.Infrastructure.BackgroundJobs;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly bool _allowAnonymous;

    public HangfireDashboardAuthorizationFilter(bool allowAnonymous)
    {
        _allowAnonymous = allowAnonymous;
    }

    public bool Authorize(DashboardContext context)
    {
        if (_allowAnonymous)
        {
            return true;
        }

        var httpContext = context.GetHttpContext();
        var currentUser = httpContext.RequestServices.GetService<ICurrentUser>();
        return currentUser?.IsAuthenticated == true;
    }
}
