using EmployeeManagementService.Domain.Enums.BoardProposal;
using FluentValidation;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.UpdateBoardProposalTaskStatus;

public class UpdateBoardProposalTaskStatusRequestValidator : AbstractValidator<UpdateBoardProposalTaskStatusRequest>
{
    public UpdateBoardProposalTaskStatusRequestValidator()
    {
        RuleFor(x => x.TaskId).GreaterThan(0);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.ExtendedDueDate)
            .NotEmpty()
            .When(x => x.Status == BoardProposalTaskStatus.Extended);
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}
