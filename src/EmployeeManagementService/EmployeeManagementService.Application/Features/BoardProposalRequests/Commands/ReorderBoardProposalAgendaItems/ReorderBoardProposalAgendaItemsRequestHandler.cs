using EmployeeManagementService.Application.Contracts;
using EmployeeManagementService.Application.Exceptions;
using EmployeeManagementService.Domain.Models.BoardProposal;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.ReorderBoardProposalAgendaItems;

public class ReorderBoardProposalAgendaItemsRequestHandler
{
    private readonly IEmployeeManagementServiceDbContext _dbContext;
    private readonly IValidator<ReorderBoardProposalAgendaItemsRequest> _validator;

    public ReorderBoardProposalAgendaItemsRequestHandler(
        IEmployeeManagementServiceDbContext dbContext,
        IValidator<ReorderBoardProposalAgendaItemsRequest> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task Handle(
        ReorderBoardProposalAgendaItemsRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ModelValidationException(validationResult.Errors);
        }

        var requestedIds = request.Items.Select(x => x.Id).ToHashSet();
        var agendaItems = await _dbContext.BoardProposalAgendaItems
            .Where(x => x.BoardProposalRequestId == request.BoardProposalRequestId
                && requestedIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (agendaItems.Count != requestedIds.Count)
        {
            throw new NotFoundException(nameof(BoardProposalAgendaItem), request.BoardProposalRequestId);
        }

        var orderById = request.Items.ToDictionary(x => x.Id, x => x.Order);
        foreach (var agendaItem in agendaItems)
        {
            agendaItem.Order = orderById[agendaItem.Id];
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
