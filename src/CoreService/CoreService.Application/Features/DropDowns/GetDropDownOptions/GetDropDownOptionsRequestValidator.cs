using FluentValidation;

namespace CoreService.Application.Features.DropDowns.GetDropDownOptions;

public class GetDropDownOptionsRequestValidator : AbstractValidator<GetDropDownOptionsRequest>
{
    public GetDropDownOptionsRequestValidator()
    {
        RuleFor(x => x.Flow).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Key).NotEmpty().MaximumLength(128);
    }
}
