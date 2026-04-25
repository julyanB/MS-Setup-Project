namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.AddBoardProposalVote;

public class AddBoardProposalVoteRequest
{
    public int AgendaItemId { get; set; }

    public string BoardMemberEmployeeId { get; set; } = null!;

    public string VoteType { get; set; } = null!;

    public string? Notes { get; set; }
}
