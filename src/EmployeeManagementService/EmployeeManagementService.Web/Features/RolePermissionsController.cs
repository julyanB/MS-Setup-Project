using EmployeeManagementService.Application.Features.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementService.Web.Features;

[ApiController]
[Route("admin/roles")]
public class RolePermissionsController : ControllerBase
{
    private readonly IRolePermissions _rolePermissions;

    public RolePermissionsController(IRolePermissions rolePermissions)
    {
        _rolePermissions = rolePermissions;
    }

    [HttpGet("/admin/permissions")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetAllPermissions(CancellationToken cancellationToken)
        => Ok(await _rolePermissions.GetAllPermissions(cancellationToken));

    [HttpPost("/admin/permissions")]
    public async Task<IActionResult> CreatePermission(
        [FromBody] CreatePermissionRequest request,
        CancellationToken cancellationToken)
    {
        await _rolePermissions.CreatePermission(request.Name, cancellationToken);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<string>>> GetRoles(CancellationToken cancellationToken)
        => Ok(await _rolePermissions.GetRoles(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> CreateRole(
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        await _rolePermissions.CreateRole(request.Name, cancellationToken);
        return NoContent();
    }

    [HttpGet("{roleName}/permissions")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetPermissions(
        string roleName,
        CancellationToken cancellationToken)
        => Ok(await _rolePermissions.GetPermissions(roleName, cancellationToken));

    [HttpPost("{roleName}/permissions")]
    public async Task<IActionResult> AddPermission(
        string roleName,
        [FromBody] AddPermissionRequest request,
        CancellationToken cancellationToken)
    {
        await _rolePermissions.AddPermission(roleName, request.Permission, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{roleName}/permissions/{permission}")]
    public async Task<IActionResult> RemovePermission(
        string roleName,
        string permission,
        CancellationToken cancellationToken)
    {
        await _rolePermissions.RemovePermission(roleName, permission, cancellationToken);
        return NoContent();
    }

    public record CreateRoleRequest(string Name);

    public record CreatePermissionRequest(string Name);

    public record AddPermissionRequest(string Permission);
}
