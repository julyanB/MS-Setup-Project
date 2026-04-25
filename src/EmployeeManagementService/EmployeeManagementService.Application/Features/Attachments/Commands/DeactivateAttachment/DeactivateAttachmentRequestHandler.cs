using EmployeeManagementService.Application.Contracts;
using EmployeeManagementService.Application.Exceptions;
using EmployeeManagementService.Domain.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementService.Application.Features.Attachments.Commands.DeactivateAttachment;

public class DeactivateAttachmentRequestHandler
{
    private readonly IEmployeeManagementServiceDbContext _dbContext;
    private readonly IValidator<DeactivateAttachmentRequest> _validator;

    public DeactivateAttachmentRequestHandler(
        IEmployeeManagementServiceDbContext dbContext,
        IValidator<DeactivateAttachmentRequest> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task Handle(
        DeactivateAttachmentRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ModelValidationException(validationResult.Errors);
        }

        var updatedRows = await _dbContext.ExecuteUpdateAsync(
            _dbContext.Attachments.Where(x => x.Id == request.Id && x.IsActive),
            setters => setters.SetProperty(x => x.IsActive, false),
            cancellationToken);

        if (updatedRows == 0)
        {
            throw new NotFoundException(nameof(Attachment), request.Id);
        }
    }
}
