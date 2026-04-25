using EmployeeManagementService.Application.Contracts;
using EmployeeManagementService.Application.Exceptions;
using EmployeeManagementService.Domain.Models;
using FluentValidation;

namespace EmployeeManagementService.Application.Features.Attachments.Commands.UploadAttachment;

public class UploadAttachmentRequestHandler
{
    private readonly IEmployeeManagementServiceDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<UploadAttachmentRequest> _validator;

    public UploadAttachmentRequestHandler(
        IEmployeeManagementServiceDbContext dbContext,
        ICurrentUser currentUser,
        IValidator<UploadAttachmentRequest> validator)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<int> Handle(
        UploadAttachmentRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ModelValidationException(validationResult.Errors);
        }

        var attachment = new Attachment
        {
            RequestType = request.RequestType,
            RequestId = request.RequestId,
            Section = request.Section,
            SectionEntityId = request.SectionEntityId,
            DocumentType = request.DocumentType,
            DocumentName = request.DocumentName,
            CustomDocumentName = request.CustomDocumentName,
            FileName = request.FileName,
            ContentType = request.ContentType,
            FileExtension = request.FileExtension,
            SizeInBytes = request.SizeInBytes,
            Content = request.Content,
            UploadedByEmployeeId = _currentUser.UserId ?? "system",
            IsActive = true
        };

        _dbContext.Attachments.Add(attachment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return attachment.Id;
    }
}
