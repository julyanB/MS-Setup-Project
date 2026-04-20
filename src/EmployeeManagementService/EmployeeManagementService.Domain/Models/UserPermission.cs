namespace EmployeeManagementService.Domain.Models;

public class UserPermission
{
    public string UserId { get; set; } = string.Empty;

    public int PermissionId { get; set; }

    public Permission Permission { get; set; } = null!;
}
