using EmployeeManagementService.Application.Contracts;
using EmployeeManagementService.Application.Exceptions;
using EmployeeManagementService.Domain.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementService.Application.Features.Attachments.Queries.DownloadAttachment;

public class DownloadAttachmentRequestHandler
{
    private readonly IEmployeeManagementServiceDbContext _dbContext;
    private readonly IValidator<DownloadAttachmentRequest> _validator;

    public DownloadAttachmentRequestHandler(
        IEmployeeManagementServiceDbContext dbContext,
        IValidator<DownloadAttachmentRequest> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<AttachmentFileDetails> Handle(
        DownloadAttachmentRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ModelValidationException(validationResult.Errors);
        }

        var file = await _dbContext.Attachments
            .AsNoTracking()
            .Where(x => x.Id == request.Id && x.IsActive)
            .Select(x => new AttachmentFileDetails
            {
                FileName = x.FileName,
                ContentType = x.ContentType,
                Content = x.Content
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (file is null)
        {
            throw new NotFoundException(nameof(Attachment), request.Id);
        }

        return file;
    }
}
