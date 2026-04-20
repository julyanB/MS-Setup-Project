using Hangfire.Dashboard;

namespace EmployeeManagementService.Infrastructure.BackgroundJobs;

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
        return httpContext.User.Identity?.IsAuthenticated == true;
    }
}
