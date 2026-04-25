namespace EmployeeManagementService.Application.Features.Attachments.Commands.UploadAttachment;

public class UploadAttachmentRequest
{
    public string RequestType { get; set; } = null!;

    public int RequestId { get; set; }

    public string? Section { get; set; }

    public int? SectionEntityId { get; set; }

    public string DocumentType { get; set; } = null!;

    public string DocumentName { get; set; } = null!;

    public string? CustomDocumentName { get; set; }

    public string FileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public string FileExtension { get; set; } = null!;

    public long SizeInBytes { get; set; }

    public byte[] Content { get; set; } = [];
}
