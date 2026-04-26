using EmployeeManagementService.Application.Services.RequestApprovalAssignment;
using EmployeeManagementService.Domain.Common;
using EmployeeManagementService.Domain.Enums;

namespace EmployeeManagementService.Application.Common;

public record ApprovalTargetMessage
{
    public ApprovalTargetMessage()
    {

    }

    public ApprovalTargetMessage(string targetType, string targetValue, string status)
    {
        TargetType = targetType;
        TargetValue = targetValue;
        Status = status;
    }

    public required string TargetType { get; init; }
    public required string TargetValue { get; init; }
    public required string Status { get; init; }
}

public static class ApprovalTargetMessageExtensions
{
    public static IReadOnlyCollection<ApprovalTargetMessage> FromAssignments(
        this IEnumerable<RequestApprovalAssignment<int>> assignments)
        => assignments
            .Select(x => new ApprovalTargetMessage
            {
                TargetType = x.TargetType.ToString(),
                TargetValue = x.TargetValue,
                Status = x.Status.ToString()
            })
            .ToArray();

    public static IReadOnlyCollection<ApprovalTargetMessage> FromAssignments(
        this IEnumerable<RequestApprovalTarget> assignments)
        => assignments
            .Select(x => new ApprovalTargetMessage
            {
                TargetType = x.TargetType.ToString(),
                TargetValue = x.TargetValue,
                Status = RequestApprovalAssignmentStatus.Active.ToString()
            })
            .ToArray();
}
