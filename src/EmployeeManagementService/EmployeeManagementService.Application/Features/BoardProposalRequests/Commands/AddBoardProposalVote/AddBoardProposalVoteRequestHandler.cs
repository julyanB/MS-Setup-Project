using EmployeeManagementService.Application.Contracts;
using EmployeeManagementService.Application.Exceptions;
using EmployeeManagementService.Application.Features.BoardProposalRequests.Common;
using EmployeeManagementService.Domain.Models;
using EmployeeManagementService.Domain.Models.BoardProposal;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.AddBoardProposalVote;

public class AddBoardProposalVoteRequestHandler
{
    private readonly IEmployeeManagementServiceDbContext _dbContext;
    private readonly IValidator<AddBoardProposalVoteRequest> _validator;

    public AddBoardProposalVoteRequestHandler(
        IEmployeeManagementServiceDbContext dbContext,
        IValidator<AddBoardProposalVoteRequest> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int> Handle(AddBoardProposalVoteRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ModelValidationException(validationResult.Errors);
        }

        var agendaItem = await _dbContext.BoardProposalAgendaItems
            .Include(x => x.Votes)
            .FirstOrDefaultAsync(x => x.Id == request.AgendaItemId, cancellationToken);

        if (agendaItem is null)
        {
            throw new NotFoundException(nameof(BoardProposalAgendaItem), request.AgendaItemId);
        }

        var vote = new BoardProposalVote
        {
            AgendaItemId = request.AgendaItemId,
            BoardMemberEmployeeId = request.BoardMemberEmployeeId,
            VoteType = request.VoteType,
            Notes = request.Notes
        };

        _dbContext.BoardProposalVotes.Add(vote);

        if (agendaItem.DecisionStatus.HasValue)
        {
            agendaItem.FinalVote = BoardProposalFinalVoteCalculator.Calculate(
                agendaItem.Votes.Append(vote));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return vote.Id;
    }
}
