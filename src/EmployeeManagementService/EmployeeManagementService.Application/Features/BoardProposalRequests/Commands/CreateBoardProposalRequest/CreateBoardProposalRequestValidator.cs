using FluentValidation;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.CreateBoardProposalRequest;

public class CreateBoardProposalRequestValidator : AbstractValidator<CreateBoardProposalRequest>
{
    public CreateBoardProposalRequestValidator()
    {
        RuleFor(x => x.MeetingDate)
            .NotEmpty();

        RuleFor(x => x.MeetingType)
            .IsInEnum();

        RuleFor(x => x.MeetingFormat)
            .IsInEnum();

        RuleFor(x => x.SecretaryEmployeeId)
            .NotEmpty()
            .MaximumLength(450);
    }
}
