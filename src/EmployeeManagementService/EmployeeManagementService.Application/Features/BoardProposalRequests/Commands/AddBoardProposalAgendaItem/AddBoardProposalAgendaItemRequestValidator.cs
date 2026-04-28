using FluentValidation;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.AddBoardProposalAgendaItem;

public class AddBoardProposalAgendaItemRequestValidator : AbstractValidator<AddBoardProposalAgendaItemRequest>
{
    public AddBoardProposalAgendaItemRequestValidator()
    {
        RuleFor(x => x.BoardProposalRequestId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.InitiatorEmployeeId).NotEmpty().MaximumLength(450);
        RuleFor(x => x.ResponsibleBoardMemberEmployeeId).NotEmpty().MaximumLength(450);
        RuleFor(x => x.PresenterEmployeeId).NotEmpty().MaximumLength(450);
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.Description).MaximumLength(1500);
    }
}
