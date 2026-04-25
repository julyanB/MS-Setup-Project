namespace EmployeeManagementService.Domain.Common;

public abstract class RequestBase<TId, TStatus> : Auditable<TId>, IRequestBase<TStatus>
    where TId : struct
{
    public TStatus? Status { get; set; }

    public ICollection<RequestApprovalAssignment<TId>> ApprovalAssignments { get; set; } = [];
}
