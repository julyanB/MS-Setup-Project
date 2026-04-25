using DOmniBus.Lite;
using EmployeeManagementService.Application.Contracts;
using EmployeeManagementService.Application.Exceptions;
using EmployeeManagementService.Application.MessageEmitters.RequestMetaDataEmitter.Update;
using EmployeeManagementService.Domain.Models.BoardProposal;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.SetBoardProposalAgendaItemDecision;

public class SetBoardProposalAgendaItemDecisionRequestHandler
{
    private readonly IEmployeeManagementServiceDbContext _dbContext;
    private readonly IValidator<SetBoardProposalAgendaItemDecisionRequest> _validator;
    private readonly IMessageBus _bus;
    private readonly ICurrentUser _currentUser;

    public SetBoardProposalAgendaItemDecisionRequestHandler(
        IEmployeeManagementServiceDbContext dbContext,
        IValidator<SetBoardProposalAgendaItemDecisionRequest> validator,
        IMessageBus bus,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _validator = validator;
        _bus = bus;
        _currentUser = currentUser;
    }

    public async Task Handle(SetBoardProposalAgendaItemDecisionRequest request, CancellationToken cancellationToken)
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

        await _dbContext.ExecuteUpdateAsync(
            _dbContext.BoardProposalAgendaItems.Where(x => x.Id == request.AgendaItemId),
            setters => setters
                .SetProperty(x => x.DecisionStatus, request.DecisionStatus)
                .SetProperty(x => x.DecisionText, request.DecisionText)
                .SetProperty(x => x.FinalVote, request.FinalVote)
                .SetProperty(x => x.Notes, request.Notes),
            cancellationToken);

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
    }
}
