using EmployeeManagementService.Domain.Enums.BoardProposal;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.SetBoardProposalAgendaItemDecision;

public class SetBoardProposalAgendaItemDecisionRequest
{
    public int AgendaItemId { get; set; }

    public BoardProposalDecisionStatus DecisionStatus { get; set; }

    public string DecisionText { get; set; } = null!;

    public string? FinalVote { get; set; }

    public string? Notes { get; set; }
}
