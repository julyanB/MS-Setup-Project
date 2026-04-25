using EmployeeManagementService.Domain.Enums.BoardProposal;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.UpdateBoardProposalTaskStatus;

public class UpdateBoardProposalTaskStatusRequest
{
    public int TaskId { get; set; }

    public BoardProposalTaskStatus Status { get; set; }

    public DateTime? ExtendedDueDate { get; set; }

    public string? Comment { get; set; }
}
