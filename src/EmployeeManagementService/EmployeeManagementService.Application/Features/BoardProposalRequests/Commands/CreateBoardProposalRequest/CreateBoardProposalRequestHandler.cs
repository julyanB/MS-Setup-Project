using System.Text.Json;
using DOmniBus.Lite;
using EmployeeManagementService.Application.Common;
using EmployeeManagementService.Application.Contracts;
using EmployeeManagementService.Application.Exceptions;
using EmployeeManagementService.Application.MessageEmitters.RequestMetaDataEmitter.Create;
using EmployeeManagementService.Application.Services.RequestApprovalAssignment;
using EmployeeManagementService.Domain.Constants;
using EmployeeManagementService.Domain.Enums;
using EmployeeManagementService.Domain.Enums.BoardProposal;
using EmployeeManagementService.Domain.Models.BoardProposal;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.CreateBoardProposalRequest;

public class CreateBoardProposalRequestHandler
{
    private const string MeetingCodePrefix = "MB";

    private readonly IEmployeeManagementServiceDbContext _dbContext;
    private readonly IValidator<CreateBoardProposalRequest> _validator;
    private readonly IMessageBus _bus;
    private readonly ICurrentUser _currentUser;
    private readonly IRequestApprovalAssignmentService _requestApprovalAssignmentService;
    private readonly ITransactionScopeService _transactionScopeService;

    public CreateBoardProposalRequestHandler(
        IEmployeeManagementServiceDbContext dbContext,
        IValidator<CreateBoardProposalRequest> validator,
        IMessageBus bus,
        ICurrentUser currentUser,
        IRequestApprovalAssignmentService requestApprovalAssignmentService,
        ITransactionScopeService transactionScopeService)
    {
        _dbContext = dbContext;
        _validator = validator;
        _bus = bus;
        _currentUser = currentUser;
        _requestApprovalAssignmentService = requestApprovalAssignmentService;
        _transactionScopeService = transactionScopeService;
    }

    public async Task<int> Handle(
        CreateBoardProposalRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ModelValidationException(validationResult.Errors);
        }

        BoardProposalRequest? boardProposalRequest = null;
        await _transactionScopeService.ExecuteInTransaction(async () =>
        {
            var dailyCount = await _dbContext.BoardProposalRequests
                .CountAsync(x => x.MeetingDate.Date == request.MeetingDate.Date, cancellationToken);

            boardProposalRequest = new BoardProposalRequest
            {
                MeetingCode = $"{MeetingCodePrefix}-{request.MeetingDate:yyyyMMdd}-{dailyCount + 1:00}",
                MeetingDate = request.MeetingDate,
                MeetingType = request.MeetingType,
                MeetingFormat = request.MeetingFormat,
                SecretaryEmployeeId = request.SecretaryEmployeeId,
                Status = BoardProposalStatus.Draft
            };

            _dbContext.BoardProposalRequests.Add(boardProposalRequest);
            await _dbContext.SaveChangesAsync(cancellationToken);

            IEnumerable<RequestApprovalTarget> approvers = [new RequestApprovalTarget(RequestApprovalTargetType.User, boardProposalRequest.SecretaryEmployeeId)];
            await _requestApprovalAssignmentService.SetActiveApprovalAssignments(
                boardProposalRequest.Id,
                nameof(BoardProposalRequest),
                approvers,
                cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            var actor = _currentUser.Email ?? _currentUser.UserId ?? "system";

            await _bus.Publish(
                new CreateRequestMetaDataEvent
                {
                    Id = boardProposalRequest.Id,
                    RequestType = nameof(BoardProposalRequest),
                    Status = boardProposalRequest.Status.ToString(),
                    CreatedBy = actor,
                    ModifiedBy = actor,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    ApprovalTargets = approvers.FromAssignments()
                },
                cancellationToken);
        });

        return boardProposalRequest!.Id;
    }
}
