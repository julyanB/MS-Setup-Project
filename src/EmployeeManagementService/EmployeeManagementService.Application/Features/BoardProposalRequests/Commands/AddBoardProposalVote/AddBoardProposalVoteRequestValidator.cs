using FluentValidation;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.AddBoardProposalVote;

public class AddBoardProposalVoteRequestValidator : AbstractValidator<AddBoardProposalVoteRequest>
{
    public AddBoardProposalVoteRequestValidator()
    {
        RuleFor(x => x.AgendaItemId).GreaterThan(0);
        RuleFor(x => x.BoardMemberEmployeeId).NotEmpty().MaximumLength(450);
        RuleFor(x => x.VoteType).IsInEnum();
        RuleFor(x => x.Notes).MaximumLength(1500);
    }
}
