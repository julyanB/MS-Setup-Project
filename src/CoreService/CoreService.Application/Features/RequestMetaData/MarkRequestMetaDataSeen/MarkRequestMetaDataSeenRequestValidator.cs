using FluentValidation;

namespace CoreService.Application.Features.RequestMetaData.MarkRequestMetaDataSeen;

public class MarkRequestMetaDataSeenRequestValidator : AbstractValidator<MarkRequestMetaDataSeenRequest>
{
    public MarkRequestMetaDataSeenRequestValidator()
    {
        RuleFor(x => x.RequestType)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
