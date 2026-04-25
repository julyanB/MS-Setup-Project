using EmployeeManagementService.Domain.Common;

namespace EmployeeManagementService.Application.Common;

public record ApprovalTargetMessage
{
    public required string TargetType { get; init; }
    public required string TargetValue { get; init; }
    public required string Status { get; init; }
}

public static class ApprovalTargetMessageFactory
{
    public static IReadOnlyCollection<ApprovalTargetMessage> FromAssignments(
        IEnumerable<RequestApprovalAssignment<int>> assignments)
        => assignments
            .Select(x => new ApprovalTargetMessage
            {
                TargetType = x.TargetType.ToString(),
                TargetValue = x.TargetValue,
                Status = x.Status.ToString()
            })
            .ToArray();
}
