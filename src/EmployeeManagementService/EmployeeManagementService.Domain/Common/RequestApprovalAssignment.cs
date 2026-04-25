using EmployeeManagementService.Domain.Enums;

namespace EmployeeManagementService.Domain.Common;

public sealed class RequestApprovalAssignment<TRequestId> : Auditable<int>
    where TRequestId : struct
{
    public TRequestId RequestId { get; set; }

    public string RequestType { get; set; } = null!;

    public RequestApprovalTargetType TargetType { get; set; }

    public string TargetValue { get; set; } = null!;

    public RequestApprovalAssignmentStatus Status { get; set; }

    public DateTimeOffset AssignedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public string? Comment { get; set; }
}
