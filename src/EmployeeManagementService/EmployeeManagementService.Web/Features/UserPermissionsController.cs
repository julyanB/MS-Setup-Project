using EmployeeManagementService.Application.Features.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementService.Web.Features;

[ApiController]
[Route("admin/users")]
public class UserPermissionsController : ControllerBase
{
    private readonly IUserPermissions _userPermissions;

    public UserPermissionsController(IUserPermissions userPermissions)
    {
        _userPermissions = userPermissions;
    }

    [HttpGet]
    public async Task<ActionResult<UserSearchResult>> GetUsers(
        [FromQuery] UserSearchRequest request,
        CancellationToken cancellationToken)
        => Ok(await _userPermissions.GetUsers(request, cancellationToken));

    [HttpGet("{userId}/permissions")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetPermissions(
        string userId,
        CancellationToken cancellationToken)
        => Ok(await _userPermissions.GetPermissions(userId, cancellationToken));

    [HttpPost("{userId}/permissions")]
    public async Task<IActionResult> AddPermission(
        string userId,
        [FromBody] AddUserPermissionRequest request,
        CancellationToken cancellationToken)
    {
        await _userPermissions.AddPermission(userId, request.Permission, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{userId}/permissions/{permission}")]
    public async Task<IActionResult> RemovePermission(
        string userId,
        string permission,
        CancellationToken cancellationToken)
    {
        await _userPermissions.RemovePermission(userId, permission, cancellationToken);
        return NoContent();
    }

    [HttpGet("{userId}/roles")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetRoles(
        string userId,
        CancellationToken cancellationToken)
        => Ok(await _userPermissions.GetRoles(userId, cancellationToken));

    [HttpPost("{userId}/roles")]
    public async Task<IActionResult> AddRole(
        string userId,
        [FromBody] AddRoleRequest request,
        CancellationToken cancellationToken)
    {
        await _userPermissions.AddRole(userId, request.RoleName, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{userId}/roles/{roleName}")]
    public async Task<IActionResult> RemoveRole(
        string userId,
        string roleName,
        CancellationToken cancellationToken)
    {
        await _userPermissions.RemoveRole(userId, roleName, cancellationToken);
        return NoContent();
    }

    public record AddUserPermissionRequest(string Permission);

    public record AddRoleRequest(string RoleName);
}
