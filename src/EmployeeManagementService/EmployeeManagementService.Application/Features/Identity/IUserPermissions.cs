namespace EmployeeManagementService.Application.Features.Identity;

public interface IUserPermissions
{
    Task<UserSearchResult> GetUsers(UserSearchRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetPermissions(string userId, CancellationToken cancellationToken = default);

    Task AddPermission(string userId, string permission, CancellationToken cancellationToken = default);

    Task RemovePermission(string userId, string permission, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetRoles(string userId, CancellationToken cancellationToken = default);

    Task AddRole(string userId, string roleName, CancellationToken cancellationToken = default);

    Task RemoveRole(string userId, string roleName, CancellationToken cancellationToken = default);
}

public sealed record UserSearchRequest(
    int Page = 1,
    int PageSize = 20,
    string? Search = null);

public sealed record UserSearchResult(
    IReadOnlyList<UserListItem> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record UserListItem(
    string Id,
    string? Email,
    string? UserName,
    int RoleCount,
    int PermissionCount);
