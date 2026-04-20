using System.Security.Claims;
using EmployeeManagementService.Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace EmployeeManagementService.Web.Services;

public class CurrentUser : ICurrentUser
{
    private const string UserPermissionClaimType = "user_permission";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public string? UserId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Name);

    public IReadOnlyCollection<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray()
        ?? Array.Empty<string>();

    public IReadOnlyCollection<string> UserPermissions =>
        Principal?.FindAll(UserPermissionClaimType).Select(c => c.Value).ToArray()
        ?? Array.Empty<string>();
}
