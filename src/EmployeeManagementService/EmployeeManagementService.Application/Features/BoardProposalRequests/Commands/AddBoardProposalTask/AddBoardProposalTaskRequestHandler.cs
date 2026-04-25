using DOmniBus.Lite;
using EmployeeManagementService.Application.Contracts;
using EmployeeManagementService.Application.Exceptions;
using EmployeeManagementService.Application.MessageEmitters.RequestMetaDataEmitter.Update;
using EmployeeManagementService.Domain.Models;
using EmployeeManagementService.Domain.Models.BoardProposal;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.AddBoardProposalTask;

public class AddBoardProposalTaskRequestHandler
{
    private readonly IEmployeeManagementServiceDbContext _dbContext;
    private readonly IValidator<AddBoardProposalTaskRequest> _validator;
    private readonly IMessageBus _bus;
    private readonly ICurrentUser _currentUser;

    public AddBoardProposalTaskRequestHandler(
        IEmployeeManagementServiceDbContext dbContext,
        IValidator<AddBoardProposalTaskRequest> validator,
        IMessageBus bus,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _validator = validator;
        _bus = bus;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(AddBoardProposalTaskRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ModelValidationException(validationResult.Errors);
        }

        var parentRequest = await _dbContext.BoardProposalAgendaItems
            .Where(x => x.Id == request.AgendaItemId)
            .Select(x => new { x.BoardProposalRequestId, x.BoardProposalRequest.Status })
            .FirstOrDefaultAsync(cancellationToken);

        if (parentRequest is null)
        {
            throw new NotFoundException(nameof(BoardProposalAgendaItem), request.AgendaItemId);
        }

        var nextOrder = await _dbContext.BoardProposalTasks
            .Where(x => x.AgendaItemId == request.AgendaItemId)
            .Select(x => (int?)x.Order)
            .MaxAsync(cancellationToken) ?? 0;

        var task = new BoardProposalTask
        {
            AgendaItemId = request.AgendaItemId,
            Title = request.Title,
            Description = request.Description,
            ResponsibleEmployeeId = request.ResponsibleEmployeeId,
            DueDate = request.DueDate,
            Order = nextOrder + 1,
            Status = request.Status
        };

        _dbContext.BoardProposalTasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var actor = _currentUser.Email ?? _currentUser.UserId ?? "system";

        await _bus.Publish(
            new UpdateRequestMetaDataEvent
            {
                Id = parentRequest.BoardProposalRequestId,
                RequestType = nameof(BoardProposalRequest),
                Status = parentRequest.Status.ToString(),
                ModifiedBy = actor,
                UpdatedAt = DateTimeOffset.UtcNow,
            },
            cancellationToken);

        return task.Id;
    }
}
