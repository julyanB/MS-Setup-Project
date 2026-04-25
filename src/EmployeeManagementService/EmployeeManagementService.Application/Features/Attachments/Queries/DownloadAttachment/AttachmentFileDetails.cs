namespace EmployeeManagementService.Application.Features.Attachments.Queries.DownloadAttachment;

public class AttachmentFileDetails
{
    public string FileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public byte[] Content { get; set; } = [];
}
