using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace EmployeeManagementService.Infrastructure.Identity.Authorization;

public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var explicitPolicy = await base.GetPolicyAsync(policyName);
        if (explicitPolicy != null)
        {
            return explicitPolicy;
        }

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(PermissionClaims.Type, policyName)
            .Build();
    }
}
