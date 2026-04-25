using FluentValidation;

namespace EmployeeManagementService.Application.Features.Attachments.Commands.UploadAttachment;

public class UploadAttachmentRequestValidator : AbstractValidator<UploadAttachmentRequest>
{
    public UploadAttachmentRequestValidator()
    {
        RuleFor(x => x.RequestType)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.RequestId)
            .GreaterThan(0);

        RuleFor(x => x.Section)
            .MaximumLength(128);

        RuleFor(x => x.DocumentType)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.DocumentName)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.CustomDocumentName)
            .MaximumLength(256);

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(260);

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.FileExtension)
            .NotEmpty()
            .MaximumLength(16);

        RuleFor(x => x.SizeInBytes)
            .GreaterThan(0);

        RuleFor(x => x.Content)
            .NotEmpty();
    }
}
