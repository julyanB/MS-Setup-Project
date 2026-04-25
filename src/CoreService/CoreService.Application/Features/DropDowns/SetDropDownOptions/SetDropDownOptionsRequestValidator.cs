using FluentValidation;

namespace CoreService.Application.Features.DropDowns.SetDropDownOptions;

public class SetDropDownOptionsRequestValidator : AbstractValidator<SetDropDownOptionsRequest>
{
    public SetDropDownOptionsRequestValidator()
    {
        RuleFor(x => x.Flow).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Key).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Body).NotNull();
        RuleFor(x => x.Body.Options).NotNull();

        RuleForEach(x => x.Body.Options).ChildRules(option =>
        {
            option.RuleFor(x => x.Code).NotEmpty().MaximumLength(128);
            option.RuleFor(x => x.Label).NotEmpty().MaximumLength(256);
            option.RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        });
    }
}
