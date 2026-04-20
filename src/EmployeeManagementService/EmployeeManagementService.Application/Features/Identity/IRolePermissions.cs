namespace EmployeeManagementService.Application.Features.Identity;

public interface IRolePermissions
{
    Task<IReadOnlyList<string>> GetPermissions(string roleName, CancellationToken cancellationToken = default);

    Task AddPermission(string roleName, string permission, CancellationToken cancellationToken = default);

    Task RemovePermission(string roleName, string permission, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetRoles(CancellationToken cancellationToken = default);

    Task CreateRole(string roleName, CancellationToken cancellationToken = default);
}
