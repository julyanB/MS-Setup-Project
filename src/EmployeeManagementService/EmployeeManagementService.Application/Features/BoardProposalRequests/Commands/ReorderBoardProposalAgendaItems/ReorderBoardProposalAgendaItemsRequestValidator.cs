using FluentValidation;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.ReorderBoardProposalAgendaItems;

public class ReorderBoardProposalAgendaItemsRequestValidator : AbstractValidator<ReorderBoardProposalAgendaItemsRequest>
{
    public ReorderBoardProposalAgendaItemsRequestValidator()
    {
        RuleFor(x => x.BoardProposalRequestId).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Id).GreaterThan(0);
            item.RuleFor(x => x.Order).GreaterThan(0);
        });
        RuleFor(x => x.Items.Select(item => item.Id))
            .Must(ids => ids.Distinct().Count() == ids.Count())
            .WithMessage("Agenda item ids must be unique.");
        RuleFor(x => x.Items.Select(item => item.Order))
            .Must(orders => orders.Distinct().Count() == orders.Count())
            .WithMessage("Agenda item order values must be unique.");
    }
}
