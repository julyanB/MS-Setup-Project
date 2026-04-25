using CoreService.Application.Contracts;
using CoreService.Application.Exceptions;
using CoreService.Domain.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CoreService.Application.Features.DropDowns.SetDropDownOptions;

public class SetDropDownOptionsRequestHandler
{
    private readonly ICoreServiceDbContext _dbContext;
    private readonly IValidator<SetDropDownOptionsRequest> _validator;

    public SetDropDownOptionsRequestHandler(
        ICoreServiceDbContext dbContext,
        IValidator<SetDropDownOptionsRequest> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task Handle(SetDropDownOptionsRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ModelValidationException(validationResult.Errors);
        }

        var existingOptions = await _dbContext.DropDownOptions
            .Where(x => x.Flow == request.Flow && x.Key == request.Key)
            .ToListAsync(cancellationToken);

        var existingByCode = existingOptions.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);
        var incomingCodes = request.Body.Options
            .Select(x => x.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var option in request.Body.Options)
        {
            if (existingByCode.TryGetValue(option.Code, out var existing))
            {
                existing.Label = option.Label;
                existing.SortOrder = option.SortOrder;
                existing.IsActive = option.IsActive;
                existing.MetadataJson = option.MetadataJson;
                continue;
            }

            _dbContext.DropDownOptions.Add(new DropDownOption
            {
                Flow = request.Flow,
                Key = request.Key,
                Code = option.Code,
                Label = option.Label,
                SortOrder = option.SortOrder,
                IsActive = option.IsActive,
                MetadataJson = option.MetadataJson
            });
        }

        foreach (var existing in existingOptions.Where(x => !incomingCodes.Contains(x.Code)))
        {
            existing.IsActive = false;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
