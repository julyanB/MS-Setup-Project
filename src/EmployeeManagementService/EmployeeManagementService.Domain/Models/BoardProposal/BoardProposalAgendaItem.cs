using System.ComponentModel;
using EmployeeManagementService.Domain.Common;
using EmployeeManagementService.Domain.Enums.BoardProposal;

namespace EmployeeManagementService.Domain.Models.BoardProposal;

public sealed class BoardProposalAgendaItem : Auditable<int>
{
    public string Title { get; set; } = null!;

    public BoardProposalAgendaCategory Category { get; set; }

    public string? Description { get; set; }

    public int Order { get; set; }

    public BoardProposalDecisionStatus? DecisionStatus { get; set; }

    public string? DecisionText { get; set; }

    public BoardProposalFinalVote? FinalVote { get; set; }

    public string? Notes { get; set; }

    // non relational properties
    [Description("UserId")]
    public string InitiatorEmployeeId { get; set; } = null!;

    [Description("UserId")]
    public string ResponsibleBoardMemberEmployeeId { get; set; } = null!;

    [Description("UserId")]
    public string PresenterEmployeeId { get; set; } = null!;

    // relational properties
    public int BoardProposalRequestId { get; set; }

    public BoardProposalRequest BoardProposalRequest { get; set; } = null!;

    public ICollection<BoardProposalVote> Votes { get; set; } = [];

    public ICollection<BoardProposalTask> Tasks { get; set; } = [];
}
