namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.SetBoardProposalAgendaItemDecision;

public class SetBoardProposalAgendaItemDecisionRequest
{
    public int AgendaItemId { get; set; }

    public string DecisionStatus { get; set; } = null!;

    public string DecisionText { get; set; } = null!;

    public string? FinalVote { get; set; }

    public string? Notes { get; set; }
}
