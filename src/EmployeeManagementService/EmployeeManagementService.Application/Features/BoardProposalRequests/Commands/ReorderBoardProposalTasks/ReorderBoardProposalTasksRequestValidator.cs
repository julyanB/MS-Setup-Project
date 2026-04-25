using FluentValidation;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.ReorderBoardProposalTasks;

public class ReorderBoardProposalTasksRequestValidator : AbstractValidator<ReorderBoardProposalTasksRequest>
{
    public ReorderBoardProposalTasksRequestValidator()
    {
        RuleFor(x => x.AgendaItemId).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Id).GreaterThan(0);
            item.RuleFor(x => x.Order).GreaterThan(0);
        });
        RuleFor(x => x.Items.Select(item => item.Id))
            .Must(ids => ids.Distinct().Count() == ids.Count())
            .WithMessage("Task ids must be unique.");
        RuleFor(x => x.Items.Select(item => item.Order))
            .Must(orders => orders.Distinct().Count() == orders.Count())
            .WithMessage("Task order values must be unique.");
    }
}
