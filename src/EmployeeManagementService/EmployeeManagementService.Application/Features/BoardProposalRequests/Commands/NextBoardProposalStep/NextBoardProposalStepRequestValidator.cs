using FluentValidation;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.NextBoardProposalStep;

public class NextBoardProposalStepRequestValidator : AbstractValidator<NextBoardProposalStepRequest>
{
    public NextBoardProposalStepRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Action)
            .IsInEnum();

        RuleFor(x => x.Comment)
            .MaximumLength(1500);
    }
}
