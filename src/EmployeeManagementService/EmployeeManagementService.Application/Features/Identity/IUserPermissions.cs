namespace EmployeeManagementService.Application.Features.Identity;

public interface IUserPermissions
{
    Task<IReadOnlyList<string>> GetPermissions(string userId, CancellationToken cancellationToken = default);

    Task AddPermission(string userId, string permission, CancellationToken cancellationToken = default);

    Task RemovePermission(string userId, string permission, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetRoles(string userId, CancellationToken cancellationToken = default);

    Task AddRole(string userId, string roleName, CancellationToken cancellationToken = default);

    Task RemoveRole(string userId, string roleName, CancellationToken cancellationToken = default);
}
