using EmployeeManagementService.Domain.Common;

namespace EmployeeManagementService.Domain.Models;

public class Permission : Entity<int>
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
    public ICollection<UserPermission> UserPermissions { get; set; } = [];
}
