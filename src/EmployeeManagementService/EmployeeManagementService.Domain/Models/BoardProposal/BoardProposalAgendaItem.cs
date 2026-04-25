using EmployeeManagementService.Domain.Common;
using EmployeeManagementService.Domain.Enums.BoardProposal;

namespace EmployeeManagementService.Domain.Models.BoardProposal;

public sealed class BoardProposalAgendaItem : Auditable<int>
{
    public int BoardProposalRequestId { get; set; }

    public BoardProposalRequest BoardProposalRequest { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string InitiatorEmployeeId { get; set; } = null!;

    public string ResponsibleBoardMemberEmployeeId { get; set; } = null!;

    public string PresenterEmployeeId { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string? Description { get; set; }

    public int Order { get; set; }

    public BoardProposalDecisionStatus? DecisionStatus { get; set; }

    public string? DecisionText { get; set; }

    public string? FinalVote { get; set; }

    public string? Notes { get; set; }

    public ICollection<BoardProposalVote> Votes { get; set; } = [];

    public ICollection<BoardProposalTask> Tasks { get; set; } = [];
}
