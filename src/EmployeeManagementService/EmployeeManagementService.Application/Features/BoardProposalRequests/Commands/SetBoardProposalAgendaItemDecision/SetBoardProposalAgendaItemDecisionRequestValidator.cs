using FluentValidation;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.SetBoardProposalAgendaItemDecision;

public class SetBoardProposalAgendaItemDecisionRequestValidator : AbstractValidator<SetBoardProposalAgendaItemDecisionRequest>
{
    public SetBoardProposalAgendaItemDecisionRequestValidator()
    {
        RuleFor(x => x.AgendaItemId).GreaterThan(0);
        RuleFor(x => x.DecisionStatus).IsInEnum();
        RuleFor(x => x.DecisionText).NotEmpty();
    }
}
