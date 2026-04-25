using FluentValidation;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.CreateBoardProposalRequest;

public class CreateBoardProposalRequestValidator : AbstractValidator<CreateBoardProposalRequest>
{
    public CreateBoardProposalRequestValidator()
    {
        RuleFor(x => x.MeetingDate)
            .NotEmpty();

        RuleFor(x => x.MeetingType)
            .NotEmpty()
            .MaximumLength(32);

        RuleFor(x => x.MeetingFormat)
            .NotEmpty()
            .MaximumLength(32);

        RuleFor(x => x.SecretaryEmployeeId)
            .NotEmpty()
            .MaximumLength(450);
    }
}
