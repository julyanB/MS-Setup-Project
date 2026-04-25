using FluentValidation;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.SetBoardProposalAgendaItemDecision;

public class SetBoardProposalAgendaItemDecisionRequestValidator : AbstractValidator<SetBoardProposalAgendaItemDecisionRequest>
{
    public SetBoardProposalAgendaItemDecisionRequestValidator()
    {
        RuleFor(x => x.AgendaItemId).GreaterThan(0);
        RuleFor(x => x.DecisionStatus).NotEmpty().MaximumLength(64);
        RuleFor(x => x.DecisionText).NotEmpty();
        RuleFor(x => x.FinalVote).MaximumLength(512);
    }
}
