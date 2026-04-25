using CoreService.Domain.Common;
using CoreService.Domain.Enums;

namespace CoreService.Domain.Models;

public class RequestMetaDataApprovalTarget : Entity<int>
{
    public required string RequestType { get; set; }

    public int RequestId { get; set; }

    public RequestApprovalTargetType TargetType { get; set; }

    public required string TargetValue { get; set; }

    public RequestApprovalAssignmentStatus Status { get; set; }

    public RequestMetaData RequestMetaData { get; set; } = null!;
}
