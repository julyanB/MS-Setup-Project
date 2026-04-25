using FluentValidation;

namespace EmployeeManagementService.Application.Features.Attachments.Queries.DownloadAttachment;

public class DownloadAttachmentRequestValidator : AbstractValidator<DownloadAttachmentRequest>
{
    public DownloadAttachmentRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
