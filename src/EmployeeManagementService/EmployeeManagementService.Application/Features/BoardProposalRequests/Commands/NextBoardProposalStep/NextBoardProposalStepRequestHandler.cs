using DOmniBus.Lite;
using EmployeeManagementService.Application.Common;
using EmployeeManagementService.Application.Contracts;
using EmployeeManagementService.Application.Exceptions;
using EmployeeManagementService.Application.MessageEmitters.RequestMetaDataEmitter.Update;
using EmployeeManagementService.Application.Services.RequestApprovalAssignment;
using EmployeeManagementService.Domain.Constants;
using EmployeeManagementService.Domain.Enums;
using EmployeeManagementService.Domain.Enums.BoardProposal;
using EmployeeManagementService.Domain.Models;
using EmployeeManagementService.Domain.Models.BoardProposal;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.NextBoardProposalStep;

public class NextBoardProposalStepRequestHandler
{
    private readonly IEmployeeManagementServiceDbContext _dbContext;
    private readonly IValidator<NextBoardProposalStepRequest> _validator;
    private readonly IMessageBus _bus;
    private readonly ICurrentUser _currentUser;
    private readonly IRequestApprovalAssignmentService _approvalAssignmentService;

    public NextBoardProposalStepRequestHandler(
        IEmployeeManagementServiceDbContext dbContext,
        IValidator<NextBoardProposalStepRequest> validator,
        IMessageBus bus,
        ICurrentUser currentUser,
        IRequestApprovalAssignmentService approvalAssignmentService)
    {
        _dbContext = dbContext;
        _validator = validator;
        _bus = bus;
        _currentUser = currentUser;
        _approvalAssignmentService = approvalAssignmentService;
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

        var attachments = await _dbContext.Attachments
            .AsNoTracking()
            .Where(x => x.RequestType == nameof(BoardProposalRequest)
                && x.RequestId == requestEntity.Id
                && x.IsActive)
            .ToListAsync(cancellationToken);

        ValidateTransitionRequirements(requestEntity, attachments, request.Action);

        var hasCompleteAgenda = HasCompleteAgenda(requestEntity, attachments);
        var hasChairpersonAgenda = HasChairpersonAgenda(requestEntity);
        var hasDecisions = HasDecisions(requestEntity);
        var hasExecutableTasks = HasExecutableTasks(requestEntity);
        var canClose = CanClose(requestEntity);

        var stateMachine = new DStateMachine.DStateMachine<EmployeeRequestAction, BoardProposalStatus>(
            requestEntity.Status);

        ConfigureStateMachine(
            stateMachine,
            hasCompleteAgenda,
            hasChairpersonAgenda,
            hasDecisions,
            hasExecutableTasks,
            canClose,
            requestEntity.MeetingDate);

        await stateMachine.TriggerAsync(request.Action);

        requestEntity.Status = stateMachine.CurrentState;
        ApplyTransitionTimestamps(requestEntity, stateMachine.CurrentState);

        var approvalAssignments = await _approvalAssignmentService.SetActiveApprovalAssignments(
            requestEntity.Id,
            nameof(BoardProposalRequest),
            GetApprovalTargetsForStatus(stateMachine.CurrentState),
            cancellationToken);

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
                ApprovalTargets = ApprovalTargetMessageExtensions.FromAssignments(approvalAssignments)
            },
            cancellationToken);
    }

    private static void ConfigureStateMachine(
        DStateMachine.DStateMachine<EmployeeRequestAction, BoardProposalStatus> stateMachine,
        bool hasCompleteAgenda,
        bool hasChairpersonAgenda,
        bool hasDecisions,
        bool hasExecutableTasks,
        bool canClose,
        DateTime meetingDate)
    {
        stateMachine.ForState(BoardProposalStatus.Draft)
            .OnTrigger(EmployeeRequestAction.Submit, t =>
                t.ChangeState(BoardProposalStatus.AgendaPreparation));

        stateMachine.ForState(BoardProposalStatus.AgendaPreparation)
            .OnTrigger(EmployeeRequestAction.Submit, t =>
                t.ChangeState(BoardProposalStatus.SecretaryReview)
                    .If(() => hasCompleteAgenda));

        stateMachine.ForState(BoardProposalStatus.SecretaryReview)
            .OnTrigger(EmployeeRequestAction.Approve, t =>
                t.ChangeState(BoardProposalStatus.ChairpersonReview)
                    .If(() => hasCompleteAgenda))
            .OnTrigger(EmployeeRequestAction.Return, t =>
                t.ChangeState(BoardProposalStatus.AgendaPreparation))
            .OnTrigger(EmployeeRequestAction.Reject, t =>
                t.ChangeState(BoardProposalStatus.Cancelled));

        stateMachine.ForState(BoardProposalStatus.ChairpersonReview)
            .OnTrigger(EmployeeRequestAction.Approve, t =>
                t.ChangeState(BoardProposalStatus.ReadyForSending)
                    .If(() => hasChairpersonAgenda))
            .OnTrigger(EmployeeRequestAction.Return, t =>
                t.ChangeState(BoardProposalStatus.SecretaryReview))
            .OnTrigger(EmployeeRequestAction.Reject, t =>
                t.ChangeState(BoardProposalStatus.Cancelled));

        stateMachine.ForState(BoardProposalStatus.ReadyForSending)
            .OnTrigger(EmployeeRequestAction.Send, t =>
                t.ChangeState(BoardProposalStatus.Sent));

        stateMachine.ForState(BoardProposalStatus.Sent)
            .OnTrigger(EmployeeRequestAction.MarkHeld, t =>
                t.ChangeState(BoardProposalStatus.Held)
                    .If(() => meetingDate <= DateTime.UtcNow));

        stateMachine.ForState(BoardProposalStatus.Held)
            .OnTrigger(EmployeeRequestAction.StartDecisionRegistration, t =>
                t.ChangeState(BoardProposalStatus.DecisionsAndTasks));

        stateMachine.ForState(BoardProposalStatus.DecisionsAndTasks)
            .OnTrigger(EmployeeRequestAction.StartMonitoring, t =>
                t.ChangeState(BoardProposalStatus.DeadlineMonitoring)
                    .If(() => hasDecisions && hasExecutableTasks));

        stateMachine.ForState(BoardProposalStatus.DeadlineMonitoring)
            .OnTrigger(EmployeeRequestAction.Close, t =>
                t.ChangeState(BoardProposalStatus.Closed)
                    .If(() => canClose));

        stateMachine.ForStates(
                BoardProposalStatus.Draft,
                BoardProposalStatus.AgendaPreparation,
                BoardProposalStatus.SecretaryReview,
                BoardProposalStatus.ChairpersonReview,
                BoardProposalStatus.ReadyForSending,
                BoardProposalStatus.Sent,
                BoardProposalStatus.Held,
                BoardProposalStatus.DecisionsAndTasks,
                BoardProposalStatus.DeadlineMonitoring)
            .OnTrigger(EmployeeRequestAction.Cancel, t =>
                t.ChangeState(BoardProposalStatus.Cancelled));

        stateMachine.ForState(BoardProposalStatus.Closed)
            .OnTrigger(EmployeeRequestAction.Reopen, t =>
                t.ChangeState(BoardProposalStatus.DeadlineMonitoring));

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

    private static IReadOnlyCollection<RequestApprovalTarget> GetApprovalTargetsForStatus(
        BoardProposalStatus status)
        => status switch
        {
            BoardProposalStatus.SecretaryReview =>
            [
                new RequestApprovalTarget(
                    RequestApprovalTargetType.Role,
                    Roles.BoardProposal_SecretaryAdmin,
                    "Secretary review")
            ],
            BoardProposalStatus.ChairpersonReview =>
            [
                new RequestApprovalTarget(
                    RequestApprovalTargetType.Role,
                    Roles.BoardProposal_BoardMember,
                    "Chairperson review")
            ],
            _ => []
        };

    private static void ValidateTransitionRequirements(
        BoardProposalRequest requestEntity,
        IReadOnlyCollection<Attachment> attachments,
        EmployeeRequestAction action)
    {
        var errors = new List<ValidationFailure>();

        switch (requestEntity.Status, action)
        {
            case (BoardProposalStatus.AgendaPreparation, EmployeeRequestAction.Submit):
            case (BoardProposalStatus.SecretaryReview, EmployeeRequestAction.Approve):
                AddAgendaValidationErrors(requestEntity, attachments, errors);
                break;

            case (BoardProposalStatus.ChairpersonReview, EmployeeRequestAction.Approve):
                if (!HasChairpersonAgenda(requestEntity))
                {
                    errors.Add(new ValidationFailure(
                        nameof(BoardProposalAgendaItem.Order),
                        "Every agenda item must have agenda order before chairperson approval."));
                }

                break;

            case (BoardProposalStatus.Sent, EmployeeRequestAction.MarkHeld):
                if (requestEntity.MeetingDate > DateTime.UtcNow)
                {
                    errors.Add(new ValidationFailure(
                        nameof(BoardProposalRequest.MeetingDate),
                        "The meeting cannot be marked as held before the meeting date."));
                }

                break;

            case (BoardProposalStatus.DecisionsAndTasks, EmployeeRequestAction.StartMonitoring):
                if (!HasDecisions(requestEntity))
                {
                    errors.Add(new ValidationFailure(
                        nameof(BoardProposalAgendaItem.DecisionStatus),
                        "Every agenda item must have a final decision before monitoring can start."));
                }

                if (!HasExecutableTasks(requestEntity))
                {
                    errors.Add(new ValidationFailure(
                        nameof(BoardProposalTask),
                        "Every approved agenda item must have at least one follow-up task before monitoring can start."));
                }

                break;

            case (BoardProposalStatus.DeadlineMonitoring, EmployeeRequestAction.Close):
                if (!CanClose(requestEntity))
                {
                    errors.Add(new ValidationFailure(
                        nameof(BoardProposalTask.Status),
                        "All required tasks must be completed, cancelled, or marked not applicable before closing."));
                }

                break;
        }

        if (errors.Count > 0)
        {
            throw new ModelValidationException(errors);
        }
    }

    private static void AddAgendaValidationErrors(
        BoardProposalRequest requestEntity,
        IReadOnlyCollection<Attachment> attachments,
        ICollection<ValidationFailure> errors)
    {
        if (requestEntity.AgendaItems.Count == 0)
        {
            errors.Add(new ValidationFailure(
                nameof(BoardProposalRequest.AgendaItems),
                "At least one agenda item is required before submitting for secretary review."));
            return;
        }

        foreach (var agendaItem in requestEntity.AgendaItems.OrderBy(x => x.Order))
        {
            var itemName = string.IsNullOrWhiteSpace(agendaItem.Title)
                ? $"Agenda item #{agendaItem.Id}"
                : agendaItem.Title;

            if (string.IsNullOrWhiteSpace(agendaItem.Title))
            {
                errors.Add(new ValidationFailure(
                    nameof(BoardProposalAgendaItem.Title),
                    $"{itemName}: title is required."));
            }

            if (string.IsNullOrWhiteSpace(agendaItem.InitiatorEmployeeId))
            {
                errors.Add(new ValidationFailure(
                    nameof(BoardProposalAgendaItem.InitiatorEmployeeId),
                    $"{itemName}: initiator is required."));
            }

            if (string.IsNullOrWhiteSpace(agendaItem.ResponsibleBoardMemberEmployeeId))
            {
                errors.Add(new ValidationFailure(
                    nameof(BoardProposalAgendaItem.ResponsibleBoardMemberEmployeeId),
                    $"{itemName}: responsible board member is required."));
            }

            if (string.IsNullOrWhiteSpace(agendaItem.PresenterEmployeeId))
            {
                errors.Add(new ValidationFailure(
                    nameof(BoardProposalAgendaItem.PresenterEmployeeId),
                    $"{itemName}: presenter is required."));
            }

            if (string.IsNullOrWhiteSpace(agendaItem.Category))
            {
                errors.Add(new ValidationFailure(
                    nameof(BoardProposalAgendaItem.Category),
                    $"{itemName}: category is required."));
            }

            if (agendaItem.Order <= 0)
            {
                errors.Add(new ValidationFailure(
                    nameof(BoardProposalAgendaItem.Order),
                    $"{itemName}: agenda order is required."));
            }

            if (!HasMaterial(agendaItem, attachments))
            {
                errors.Add(new ValidationFailure(
                    nameof(Attachment),
                    $"{itemName}: at least one material attachment is required."));
            }
        }
    }

    private static bool HasCompleteAgenda(
        BoardProposalRequest requestEntity,
        IReadOnlyCollection<Attachment> attachments)
    {
        if (requestEntity.AgendaItems.Count == 0)
        {
            return false;
        }

        return requestEntity.AgendaItems.All(agendaItem =>
            !string.IsNullOrWhiteSpace(agendaItem.Title)
            && !string.IsNullOrWhiteSpace(agendaItem.InitiatorEmployeeId)
            && !string.IsNullOrWhiteSpace(agendaItem.ResponsibleBoardMemberEmployeeId)
            && !string.IsNullOrWhiteSpace(agendaItem.PresenterEmployeeId)
            && !string.IsNullOrWhiteSpace(agendaItem.Category)
            && agendaItem.Order > 0
            && HasMaterial(agendaItem, attachments));
    }

    private static bool HasChairpersonAgenda(BoardProposalRequest requestEntity)
        => requestEntity.AgendaItems.Count > 0
            && requestEntity.AgendaItems.All(x => x.Order > 0);

    private static bool HasDecisions(BoardProposalRequest requestEntity)
        => requestEntity.AgendaItems.Count > 0
            && requestEntity.AgendaItems.All(x => x.DecisionStatus.HasValue);

    private static bool HasExecutableTasks(BoardProposalRequest requestEntity)
        => requestEntity.AgendaItems
            .Where(x => x.DecisionStatus == BoardProposalDecisionStatus.Approved)
            .All(x => x.Tasks.Count > 0);

    private static bool CanClose(BoardProposalRequest requestEntity)
    {
        var tasks = requestEntity.AgendaItems.SelectMany(x => x.Tasks).ToList();

        return tasks.All(task =>
            task.Status is BoardProposalTaskStatus.Completed
                or BoardProposalTaskStatus.Cancelled
                or BoardProposalTaskStatus.NotApplicable);
    }

    private static bool HasMaterial(
        BoardProposalAgendaItem agendaItem,
        IReadOnlyCollection<Attachment> attachments)
        => attachments.Any(attachment =>
            string.Equals(attachment.Section, "AgendaItem", StringComparison.OrdinalIgnoreCase)
            && attachment.SectionEntityId == agendaItem.Id);
}
