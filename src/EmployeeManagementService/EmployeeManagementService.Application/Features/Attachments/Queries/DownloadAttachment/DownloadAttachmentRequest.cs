using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementService.Application.Features.Attachments.Queries.DownloadAttachment;

public class DownloadAttachmentRequest
{
    [FromRoute(Name = "id")]
    public int Id { get; set; }
}
