using EmployeeManagementService.Domain.Common;

namespace EmployeeManagementService.Domain.Models;

public sealed class Attachment : Auditable<int>
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

    public string UploadedByEmployeeId { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}
