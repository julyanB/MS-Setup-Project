using DOmniBus.Lite;
using EmployeeManagementService.Application.Contracts;
using EmployeeManagementService.Application.Exceptions;
using EmployeeManagementService.Application.MessageEmitters.RequestMetaDataEmitter.Update;
using EmployeeManagementService.Domain.Enums;
using EmployeeManagementService.Domain.Models.BoardProposal;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.NextBoardProposalStep;

public class NextBoardProposalStepRequestHandler
{
    private readonly IEmployeeManagementServiceDbContext _dbContext;
    private readonly IValidator<NextBoardProposalStepRequest> _validator;
    private readonly IMessageBus _bus;
    private readonly ICurrentUser _currentUser;

    public NextBoardProposalStepRequestHandler(
        IEmployeeManagementServiceDbContext dbContext,
        IValidator<NextBoardProposalStepRequest> validator,
        IMessageBus bus,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _validator = validator;
        _bus = bus;
        _currentUser = currentUser;
    }

    public async Task Handle(
        NextBoardProposalStepRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ModelValidationException(validationResult.Errors);
        }

        var requestEntity = await _dbContext.BoardProposalRequests
            .Include(x => x.AgendaItems)
            .ThenInclude(x => x.Tasks)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (requestEntity is null)
        {
            throw new NotFoundException(nameof(BoardProposalRequest), request.Id);
        }

        var hasAgendaItems = requestEntity.AgendaItems.Count > 0;
        var hasDecisions = requestEntity.AgendaItems.All(x => !string.IsNullOrWhiteSpace(x.DecisionStatus));
        var hasTasks = requestEntity.AgendaItems.SelectMany(x => x.Tasks).Any();

        var stateMachine = new DStateMachine.DStateMachine<EmployeeRequestAction, BoardProposalStatus>(
            requestEntity.Status);

        ConfigureStateMachine(stateMachine, hasAgendaItems, hasDecisions, hasTasks, requestEntity.MeetingDate);

        await stateMachine.TriggerAsync(request.Action);

        requestEntity.Status = stateMachine.CurrentState;
        ApplyTransitionTimestamps(requestEntity, stateMachine.CurrentState);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var actor = _currentUser.Email ?? _currentUser.UserId ?? "system";

        await _bus.Publish(
            new UpdateRequestMetaDataEvent
            {
                Id = requestEntity.Id,
                RequestType = nameof(BoardProposalRequest),
                Status = requestEntity.Status.ToString(),
                ModifiedBy = actor,
                UpdatedAt = DateTimeOffset.UtcNow,
            },
            cancellationToken);
    }

    private static void ConfigureStateMachine(
        DStateMachine.DStateMachine<EmployeeRequestAction, BoardProposalStatus> stateMachine,
        bool hasAgendaItems,
        bool hasDecisions,
        bool hasTasks,
        DateTime meetingDate)
    {
        stateMachine.ForState(BoardProposalStatus.Draft)
            .OnTrigger(EmployeeRequestAction.Submit, t =>
                t.ChangeState(BoardProposalStatus.AgendaPreparation));

        stateMachine.ForState(BoardProposalStatus.AgendaPreparation)
            .OnTrigger(EmployeeRequestAction.MoveNext, t =>
                t.ChangeState(BoardProposalStatus.MaterialsPreparation)
                    .If(() => hasAgendaItems));

        stateMachine.ForState(BoardProposalStatus.MaterialsPreparation)
            .OnTrigger(EmployeeRequestAction.MoveNext, t =>
                t.ChangeState(BoardProposalStatus.ReadyForReview));

        stateMachine.ForState(BoardProposalStatus.ReadyForReview)
            .OnTrigger(EmployeeRequestAction.Send, t =>
                t.ChangeState(BoardProposalStatus.Sent));

        stateMachine.ForState(BoardProposalStatus.Sent)
            .OnTrigger(EmployeeRequestAction.MarkHeld, t =>
                t.ChangeState(BoardProposalStatus.Held)
                    .If(() => meetingDate <= DateTime.UtcNow));

        stateMachine.ForState(BoardProposalStatus.Held)
            .OnTrigger(EmployeeRequestAction.RegisterDecisions, t =>
                t.ChangeState(BoardProposalStatus.DecisionsRegistration));

        stateMachine.ForState(BoardProposalStatus.DecisionsRegistration)
            .OnTrigger(EmployeeRequestAction.StartMonitoring, t =>
                t.ChangeState(BoardProposalStatus.TaskMonitoring)
                    .If(() => hasDecisions));

        stateMachine.ForState(BoardProposalStatus.TaskMonitoring)
            .OnTrigger(EmployeeRequestAction.Close, t =>
                t.ChangeState(BoardProposalStatus.Closed)
                    .If(() => hasDecisions && hasTasks));

        stateMachine.ForStates(
                BoardProposalStatus.Draft,
                BoardProposalStatus.AgendaPreparation,
                BoardProposalStatus.MaterialsPreparation,
                BoardProposalStatus.ReadyForReview,
                BoardProposalStatus.Sent,
                BoardProposalStatus.Held,
                BoardProposalStatus.DecisionsRegistration,
                BoardProposalStatus.TaskMonitoring)
            .OnTrigger(EmployeeRequestAction.Cancel, t =>
                t.ChangeState(BoardProposalStatus.Cancelled));

        stateMachine.ForState(BoardProposalStatus.Closed)
            .OnTrigger(EmployeeRequestAction.Reopen, t =>
                t.ChangeState(BoardProposalStatus.TaskMonitoring));

        stateMachine.OnUnhandledTrigger((trigger, machine) =>
        {
            throw new InvalidOperationException(
                $"Action '{trigger}' cannot be executed from status '{machine.CurrentState}'.");
        });
    }

    private static void ApplyTransitionTimestamps(
        BoardProposalRequest requestEntity,
        BoardProposalStatus status)
    {
        var now = DateTime.UtcNow;

        switch (status)
        {
            case BoardProposalStatus.Sent:
                requestEntity.SentAt ??= now;
                break;
            case BoardProposalStatus.Held:
                requestEntity.HeldAt ??= now;
                break;
            case BoardProposalStatus.Closed:
                requestEntity.ClosedAt ??= now;
                break;
        }
    }
}
