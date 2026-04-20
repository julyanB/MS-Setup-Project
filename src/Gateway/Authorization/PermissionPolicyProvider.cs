using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Gateway.Authorization;

/// <summary>
/// Dynamically creates authorization policies for any policy name
/// prefixed with "permission:". This lets YARP route config declare
/// per-permission access without any code changes:
///
///   "AuthorizationPolicy": "permission:banking.payments.read"
///
/// Falls through to the default provider for all other policy names
/// (e.g. "authenticated"), so existing routes are unaffected.
/// </summary>
public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private const string Prefix = "permission:";

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return await base.GetPolicyAsync(policyName);
        }

        var permissionName = policyName[Prefix.Length..];

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim("permission", permissionName)
            .Build();
    }
}
