using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementService.Application.Features.Attachments.Commands.DeactivateAttachment;

public class DeactivateAttachmentRequest
{
    [FromRoute(Name = "id")]
    public int Id { get; set; }
}
