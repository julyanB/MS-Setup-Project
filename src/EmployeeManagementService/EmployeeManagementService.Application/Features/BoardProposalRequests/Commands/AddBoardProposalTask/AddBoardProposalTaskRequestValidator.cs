using FluentValidation;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.AddBoardProposalTask;

public class AddBoardProposalTaskRequestValidator : AbstractValidator<AddBoardProposalTaskRequest>
{
    public AddBoardProposalTaskRequestValidator()
    {
        RuleFor(x => x.AgendaItemId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(512);
        RuleFor(x => x.ResponsibleEmployeeId).NotEmpty().MaximumLength(450);
        RuleFor(x => x.DueDate).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}
