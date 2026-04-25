using DOmniBus.Lite;
using EmployeeManagementService.Application.Contracts;
using EmployeeManagementService.Application.Exceptions;
using EmployeeManagementService.Application.MessageEmitters.RequestMetaDataEmitter.Update;
using EmployeeManagementService.Domain.Models;
using EmployeeManagementService.Domain.Models.BoardProposal;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.AddBoardProposalAgendaItem;

public class AddBoardProposalAgendaItemRequestHandler
{
    private readonly IEmployeeManagementServiceDbContext _dbContext;
    private readonly IValidator<AddBoardProposalAgendaItemRequest> _validator;
    private readonly IMessageBus _bus;
    private readonly ICurrentUser _currentUser;

    public AddBoardProposalAgendaItemRequestHandler(
        IEmployeeManagementServiceDbContext dbContext,
        IValidator<AddBoardProposalAgendaItemRequest> validator,
        IMessageBus bus,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _validator = validator;
        _bus = bus;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(AddBoardProposalAgendaItemRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ModelValidationException(validationResult.Errors);
        }

        var parentRequest = await _dbContext.BoardProposalRequests
            .FirstOrDefaultAsync(x => x.Id == request.BoardProposalRequestId, cancellationToken);
        if (parentRequest is null)
        {
            throw new NotFoundException(nameof(BoardProposalRequest), request.BoardProposalRequestId);
        }

        var nextOrder = await _dbContext.BoardProposalAgendaItems
            .Where(x => x.BoardProposalRequestId == request.BoardProposalRequestId)
            .Select(x => (int?)x.Order)
            .MaxAsync(cancellationToken) ?? 0;

        var agendaItem = new BoardProposalAgendaItem
        {
            BoardProposalRequestId = request.BoardProposalRequestId,
            Title = request.Title,
            InitiatorEmployeeId = request.InitiatorEmployeeId,
            ResponsibleBoardMemberEmployeeId = request.ResponsibleBoardMemberEmployeeId,
            PresenterEmployeeId = request.PresenterEmployeeId,
            Category = request.Category,
            Description = request.Description,
            Order = nextOrder + 1
        };

        _dbContext.BoardProposalAgendaItems.Add(agendaItem);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var actor = _currentUser.Email ?? _currentUser.UserId ?? "system";

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

        return agendaItem.Id;
    }
}
