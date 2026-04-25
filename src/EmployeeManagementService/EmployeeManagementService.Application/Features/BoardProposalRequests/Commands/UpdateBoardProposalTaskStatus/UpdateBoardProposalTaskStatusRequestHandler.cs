using DOmniBus.Lite;
using EmployeeManagementService.Application.Contracts;
using EmployeeManagementService.Application.Exceptions;
using EmployeeManagementService.Application.MessageEmitters.RequestMetaDataEmitter.Update;
using EmployeeManagementService.Domain.Models.BoardProposal;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.UpdateBoardProposalTaskStatus;

public class UpdateBoardProposalTaskStatusRequestHandler
{
    private readonly IEmployeeManagementServiceDbContext _dbContext;
    private readonly IValidator<UpdateBoardProposalTaskStatusRequest> _validator;
    private readonly IMessageBus _bus;
    private readonly ICurrentUser _currentUser;

    public UpdateBoardProposalTaskStatusRequestHandler(
        IEmployeeManagementServiceDbContext dbContext,
        IValidator<UpdateBoardProposalTaskStatusRequest> validator,
        IMessageBus bus,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _validator = validator;
        _bus = bus;
        _currentUser = currentUser;
    }

    public async Task Handle(
        UpdateBoardProposalTaskStatusRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ModelValidationException(validationResult.Errors);
        }

        var task = await _dbContext.BoardProposalTasks
            .Include(x => x.AgendaItem)
            .ThenInclude(x => x.BoardProposalRequest)
            .FirstOrDefaultAsync(x => x.Id == request.TaskId, cancellationToken);

        if (task is null)
        {
            throw new NotFoundException(nameof(BoardProposalTask), request.TaskId);
        }

        task.Status = request.Status;
        task.ExtendedDueDate = request.ExtendedDueDate;
        task.Comment = request.Comment;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var actor = _currentUser.Email ?? _currentUser.UserId ?? "system";
        var parentRequest = task.AgendaItem.BoardProposalRequest;

        await _bus.Publish(
            new UpdateRequestMetaDataEvent
            {
                Id = parentRequest.Id,
                RequestType = nameof(BoardProposalRequest),
                Status = parentRequest.Status.ToString(),
                ModifiedBy = actor,
                UpdatedAt = DateTimeOffset.UtcNow,
            },
            cancellationToken);
    }
}
