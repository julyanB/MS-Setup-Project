using DOmniBus.Lite;
using EmployeeManagementService.Application.Contracts;
using EmployeeManagementService.Application.Exceptions;
using EmployeeManagementService.Application.Features.BoardProposalRequests.Common;
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

        var agendaItem = await _dbContext.BoardProposalAgendaItems
            .Include(x => x.Votes)
            .Include(x => x.BoardProposalRequest)
            .Where(x => x.Id == request.AgendaItemId)
            .FirstOrDefaultAsync(cancellationToken);

        if (agendaItem is null)
        {
            throw new NotFoundException(nameof(BoardProposalAgendaItem), request.AgendaItemId);
        }

        agendaItem.DecisionStatus = request.DecisionStatus;
        agendaItem.DecisionText = request.DecisionText;
        agendaItem.FinalVote = BoardProposalFinalVoteCalculator.Calculate(agendaItem.Votes);
        agendaItem.Notes = request.Notes;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var actor = _currentUser.Email ?? _currentUser.UserId ?? "system";

        await _bus.Publish(
            new UpdateRequestMetaDataEvent
            {
                Id = agendaItem.BoardProposalRequestId,
                RequestType = nameof(BoardProposalRequest),
                Status = agendaItem.BoardProposalRequest.Status.ToString(),
                ModifiedBy = actor,
                UpdatedAt = DateTimeOffset.UtcNow,
            },
            cancellationToken);
    }
}
