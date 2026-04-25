using EmployeeManagementService.Domain.Common;

namespace EmployeeManagementService.Application.Services.RequestApprovalAssignment;

public interface IRequestApprovalAssignmentService
{
    Task<IReadOnlyCollection<RequestApprovalAssignment<int>>> GetActiveApprovalAssignments(int requestId, string requestType, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<RequestApprovalAssignment<int>>> SetActiveApprovalAssignments(int requestId, string requestType, IEnumerable<RequestApprovalTarget> targets, CancellationToken cancellationToken);
}
