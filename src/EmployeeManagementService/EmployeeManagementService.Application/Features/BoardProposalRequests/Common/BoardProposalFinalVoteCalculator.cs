using EmployeeManagementService.Domain.Enums.BoardProposal;
using EmployeeManagementService.Domain.Models.BoardProposal;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Common;

public static class BoardProposalFinalVoteCalculator
{
    public static BoardProposalFinalVote Calculate(IEnumerable<BoardProposalVote> votes)
    {
        var positive = 0;
        var negative = 0;

        foreach (var vote in votes)
        {
            if (IsPositive(vote.VoteType))
            {
                positive++;
            }
            else if (IsNegative(vote.VoteType))
            {
                negative++;
            }
        }

        if (positive == 0 && negative == 0)
        {
            return BoardProposalFinalVote.NoVotes;
        }

        if (positive == negative)
        {
            return BoardProposalFinalVote.Tie;
        }

        return positive > negative
            ? BoardProposalFinalVote.Positive
            : BoardProposalFinalVote.Negative;
    }

    private static bool IsPositive(BoardProposalVoteType voteType)
        => voteType is BoardProposalVoteType.Positive
            or BoardProposalVoteType.PositiveWithCondition
            or BoardProposalVoteType.PositiveWithRecommendation;

    private static bool IsNegative(BoardProposalVoteType voteType)
        => voteType is BoardProposalVoteType.Negative
            or BoardProposalVoteType.NegativeWithComments;
}
