using EmployeeManagementService.Domain.Enums.BoardProposal;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.AddBoardProposalTask;

public class AddBoardProposalTaskRequest
{
    public int AgendaItemId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string ResponsibleEmployeeId { get; set; } = null!;

    public DateTimeOffset DueDate { get; set; }

    public BoardProposalTaskStatus Status { get; set; } = BoardProposalTaskStatus.ToDo;
}
