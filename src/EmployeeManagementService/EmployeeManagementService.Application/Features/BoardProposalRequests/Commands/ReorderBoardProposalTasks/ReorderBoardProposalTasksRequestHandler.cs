using EmployeeManagementService.Application.Contracts;
using EmployeeManagementService.Application.Exceptions;
using EmployeeManagementService.Domain.Models.BoardProposal;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.ReorderBoardProposalTasks;

public class ReorderBoardProposalTasksRequestHandler
{
    private readonly IEmployeeManagementServiceDbContext _dbContext;
    private readonly IValidator<ReorderBoardProposalTasksRequest> _validator;

    public ReorderBoardProposalTasksRequestHandler(
        IEmployeeManagementServiceDbContext dbContext,
        IValidator<ReorderBoardProposalTasksRequest> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task Handle(ReorderBoardProposalTasksRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ModelValidationException(validationResult.Errors);
        }

        var requestedIds = request.Items.Select(x => x.Id).ToHashSet();
        var tasks = await _dbContext.BoardProposalTasks
            .Where(x => x.AgendaItemId == request.AgendaItemId && requestedIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (tasks.Count != requestedIds.Count)
        {
            throw new NotFoundException(nameof(BoardProposalTask), request.AgendaItemId);
        }

        var orderById = request.Items.ToDictionary(x => x.Id, x => x.Order);
        foreach (var task in tasks)
        {
            task.Order = orderById[task.Id];
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
