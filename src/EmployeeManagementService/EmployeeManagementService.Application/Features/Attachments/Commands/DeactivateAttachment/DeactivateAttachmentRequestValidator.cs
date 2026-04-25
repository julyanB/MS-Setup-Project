using FluentValidation;

namespace EmployeeManagementService.Application.Features.Attachments.Commands.DeactivateAttachment;

public class DeactivateAttachmentRequestValidator : AbstractValidator<DeactivateAttachmentRequest>
{
    public DeactivateAttachmentRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
