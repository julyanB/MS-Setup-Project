using CoreService.Application.Contracts;
using CoreService.Application.Exceptions;
using FluentValidation;

namespace CoreService.Application.Features.RequestMetaData.MarkRequestMetaDataSeen;

public class MarkRequestMetaDataSeenRequestHandler
{
    private readonly ICoreServiceDbContext _dbContext;
    private readonly IValidator<MarkRequestMetaDataSeenRequest> _validator;

    public MarkRequestMetaDataSeenRequestHandler(
        ICoreServiceDbContext dbContext,
        IValidator<MarkRequestMetaDataSeenRequest> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task Handle(
        MarkRequestMetaDataSeenRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ModelValidationException(validationResult.Errors);
        }

        var updatedRows = await _dbContext.ExecuteUpdateAsync(
            _dbContext.RequestMetaData.Where(r => r.Id == request.Id && r.RequestType == request.RequestType),
            x => x.SetProperty(p => p.Seen, true),
            cancellationToken);

        if (updatedRows == 0)
        {
            throw new NotFoundException(nameof(Domain.Models.RequestMetaData), $"{request.RequestType}/{request.Id}");
        }
    }
}
