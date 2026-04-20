namespace EmployeeManagementService.Domain.Common;

public abstract class Auditable<TId> : Trackable<TId>, IAuditable
    where TId : struct
{
    public string? CreatedBy { get; set; }

    public string? ModifiedBy { get; set; }
}
