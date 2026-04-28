using EmployeeManagementService.Domain.Enums.BoardProposal;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.AddBoardProposalVote;

public class AddBoardProposalVoteRequest
{
    public int AgendaItemId { get; set; }

    public string BoardMemberEmployeeId { get; set; } = null!;

    public BoardProposalVoteType VoteType { get; set; }

    public string? Notes { get; set; }
}
