using System.ComponentModel;
using EmployeeManagementService.Domain.Common;
using EmployeeManagementService.Domain.Enums.BoardProposal;

namespace EmployeeManagementService.Domain.Models.BoardProposal;

public sealed class BoardProposalVote : Auditable<int>
{
    public int AgendaItemId { get; set; }

    public BoardProposalAgendaItem AgendaItem { get; set; } = null!;

    [Description("UserId")]
    public string BoardMemberEmployeeId { get; set; } = null!;

    public BoardProposalVoteType VoteType { get; set; }

    public string? Notes { get; set; }
}
