using EmployeeManagementService.Application.Contracts;
using EmployeeManagementService.Domain.Common;
using EmployeeManagementService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementService.Application.Services.RequestApprovalAssignment;

public sealed record RequestApprovalTarget(
    RequestApprovalTargetType TargetType,
    string TargetValue,
    string? Comment = null);

public class RequestApprovalAssignmentService : IRequestApprovalAssignmentService
{
    private readonly IEmployeeManagementServiceDbContext _dbContext;

    public RequestApprovalAssignmentService(IEmployeeManagementServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<RequestApprovalAssignment<int>>> SetActiveApprovalAssignments(
        int requestId,
        string requestType,
        IEnumerable<RequestApprovalTarget> targets,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var existingActiveAssignments = await _dbContext.RequestApprovalAssignments
            .Where(x => x.RequestId == requestId
                && x.RequestType == requestType
                && x.Status == RequestApprovalAssignmentStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var assignment in existingActiveAssignments)
        {
            assignment.Status = RequestApprovalAssignmentStatus.Completed;
            assignment.CompletedAt = now;
        }

        var newAssignments = targets
            .Where(x => !string.IsNullOrWhiteSpace(x.TargetValue))
            .Select(x => new RequestApprovalAssignment<int>
            {
                RequestId = requestId,
                RequestType = requestType,
                TargetType = x.TargetType,
                TargetValue = x.TargetValue.Trim(),
                Status = RequestApprovalAssignmentStatus.Active,
                AssignedAt = now,
                Comment = x.Comment
            })
            .ToList();

        if (newAssignments.Count > 0)
        {
            await _dbContext.RequestApprovalAssignments.AddRangeAsync(newAssignments, cancellationToken);
        }

        return newAssignments;
    }

    public async Task<IReadOnlyCollection<RequestApprovalAssignment<int>>> GetActiveApprovalAssignments(
        int requestId,
        string requestType,
        CancellationToken cancellationToken)
        => await _dbContext.RequestApprovalAssignments
            .AsNoTracking()
            .Where(x => x.RequestId == requestId
                && x.RequestType == requestType
                && x.Status == RequestApprovalAssignmentStatus.Active)
            .ToListAsync(cancellationToken);
}
