using EmployeeManagementService.Application.Contracts;
using EmployeeManagementService.Application.Exceptions;
using EmployeeManagementService.Domain.Enums;
using EmployeeManagementService.Domain.Models.BoardProposal;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Queries.SearchBoardProposalRequest;

public class SearchBoardProposalRequestHandler
{
    private readonly IEmployeeManagementServiceDbContext _dbContext;
    private readonly IValidator<SearchBoardProposalRequest> _validator;

    public SearchBoardProposalRequestHandler(
        IEmployeeManagementServiceDbContext dbContext,
        IValidator<SearchBoardProposalRequest> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<BoardProposalRequestDetails> Handle(
        SearchBoardProposalRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ModelValidationException(validationResult.Errors);
        }

        var details = await _dbContext.BoardProposalRequests
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new BoardProposalRequestDetails
            {
                Id = x.Id,
                MeetingCode = x.MeetingCode,
                MeetingDate = x.MeetingDate,
                MeetingType = x.MeetingType,
                MeetingFormat = x.MeetingFormat,
                SecretaryEmployeeId = x.SecretaryEmployeeId,
                Status = x.Status,
                AgendaItems = x.AgendaItems
                    .OrderBy(a => a.Order)
                    .Select(a => new BoardProposalAgendaItemDetails
                    {
                        Id = a.Id,
                        Title = a.Title,
                        InitiatorEmployeeId = a.InitiatorEmployeeId,
                        ResponsibleBoardMemberEmployeeId = a.ResponsibleBoardMemberEmployeeId,
                        PresenterEmployeeId = a.PresenterEmployeeId,
                        Category = a.Category,
                        Description = a.Description,
                        Order = a.Order,
                        DecisionStatus = a.DecisionStatus,
                        DecisionText = a.DecisionText,
                        FinalVote = a.FinalVote,
                        Notes = a.Notes,
                        Votes = a.Votes
                            .Select(v => new BoardProposalVoteDetails
                            {
                                Id = v.Id,
                                BoardMemberEmployeeId = v.BoardMemberEmployeeId,
                                VoteType = v.VoteType,
                                Notes = v.Notes
                            })
                            .ToList(),
                        Tasks = a.Tasks
                            .OrderBy(t => t.Order)
                            .ThenBy(t => t.DueDate)
                            .Select(t => new BoardProposalTaskDetails
                            {
                                Id = t.Id,
                                Title = t.Title,
                                Description = t.Description,
                                ResponsibleEmployeeId = t.ResponsibleEmployeeId,
                                DueDate = t.DueDate,
                                Order = t.Order,
                                Status = t.Status,
                                ExtendedDueDate = t.ExtendedDueDate,
                                Comment = t.Comment
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (details is null)
        {
            throw new NotFoundException(nameof(BoardProposalRequest), request.Id);
        }

        details.Attachments = await _dbContext.Attachments
            .AsNoTracking()
            .Where(x => x.RequestType == nameof(BoardProposalRequest)
                && x.RequestId == request.Id
                && x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AttachmentDetails
            {
                Id = x.Id,
                FileName = x.FileName,
                DocumentType = x.DocumentType,
                DocumentName = x.DocumentName,
                Section = x.Section,
                SectionEntityId = x.SectionEntityId,
                SizeInBytes = x.SizeInBytes,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return details;
    }
}
