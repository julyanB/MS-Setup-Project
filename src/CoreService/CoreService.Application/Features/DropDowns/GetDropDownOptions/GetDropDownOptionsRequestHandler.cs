using CoreService.Application.Contracts;
using CoreService.Application.Exceptions;
using CoreService.Application.Features.DropDowns;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CoreService.Application.Features.DropDowns.GetDropDownOptions;

public class GetDropDownOptionsRequestHandler
{
    private readonly ICoreServiceDbContext _dbContext;
    private readonly IValidator<GetDropDownOptionsRequest> _validator;

    public GetDropDownOptionsRequestHandler(
        ICoreServiceDbContext dbContext,
        IValidator<GetDropDownOptionsRequest> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<IReadOnlyCollection<DropDownOptionDetails>> Handle(
        GetDropDownOptionsRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ModelValidationException(validationResult.Errors);
        }

        var query = _dbContext.DropDownOptions
            .AsNoTracking()
            .Where(x => x.Flow == request.Flow && x.Key == request.Key);

        if (!request.IncludeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Label)
            .Select(x => new DropDownOptionDetails
            {
                Id = x.Id,
                Flow = x.Flow,
                Key = x.Key,
                Code = x.Code,
                Label = x.Label,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                MetadataJson = x.MetadataJson
            })
            .ToListAsync(cancellationToken);
    }
}
