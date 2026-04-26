using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Gateway.Authorization;

/// <summary>
/// Dynamically creates authorization policies for any policy name
/// prefixed with "permission:". This lets YARP route config declare
/// per-permission access without any code changes:
///
///   "AuthorizationPolicy": "permission:banking.payments.read"
///   "AuthorizationPolicy": "permission:any:banking.payments.read,banking.payments.write"
///   "AuthorizationPolicy": "permission:all:banking.payments.read,banking.payments.write"
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

        var permissionExpression = policyName[Prefix.Length..];
        var rule = PermissionRule.Parse(permissionExpression);

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireAssertion(context => rule.IsSatisfiedBy(
                context.User.FindAll("permission").Select(claim => claim.Value)))
            .Build();
    }

    private sealed record PermissionRule(
        PermissionRuleMode Mode,
        IReadOnlyCollection<string> Permissions)
    {
        public static PermissionRule Parse(string expression)
        {
            var mode = PermissionRuleMode.All;
            var permissionsExpression = expression;

            if (expression.StartsWith("any:", StringComparison.OrdinalIgnoreCase))
            {
                mode = PermissionRuleMode.Any;
                permissionsExpression = expression["any:".Length..];
            }
            else if (expression.StartsWith("all:", StringComparison.OrdinalIgnoreCase))
            {
                mode = PermissionRuleMode.All;
                permissionsExpression = expression["all:".Length..];
            }

            var permissions = permissionsExpression
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(permission => !string.IsNullOrWhiteSpace(permission))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return new PermissionRule(mode, permissions);
        }

        public bool IsSatisfiedBy(IEnumerable<string> userPermissions)
        {
            if (Permissions.Count == 0)
            {
                return false;
            }

            var userPermissionSet = userPermissions.ToHashSet(StringComparer.OrdinalIgnoreCase);

            return Mode switch
            {
                PermissionRuleMode.Any => Permissions.Any(userPermissionSet.Contains),
                PermissionRuleMode.All => Permissions.All(userPermissionSet.Contains),
                _ => false
            };
        }
    }

    private enum PermissionRuleMode
    {
        Any,
        All
    }
}
