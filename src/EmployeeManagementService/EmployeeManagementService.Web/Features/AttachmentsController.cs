using EmployeeManagementService.Application.Features.Attachments.Commands.DeactivateAttachment;
using EmployeeManagementService.Application.Features.Attachments.Commands.UploadAttachment;
using EmployeeManagementService.Application.Features.Attachments.Queries.DownloadAttachment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementService.Web.Features;

[ApiController]
[Route("attachments")]
public class AttachmentsController : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<int>> Upload(
        [FromForm] UploadAttachmentForm form,
        [FromServices] UploadAttachmentRequestHandler requestHandler,
        CancellationToken cancellationToken)
    {
        await using var stream = form.File.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);

        var request = new UploadAttachmentRequest
        {
            RequestType = form.RequestType,
            RequestId = form.RequestId,
            Section = form.Section,
            SectionEntityId = form.SectionEntityId,
            DocumentType = form.DocumentType,
            DocumentName = form.DocumentName,
            CustomDocumentName = form.CustomDocumentName,
            FileName = form.File.FileName,
            ContentType = form.File.ContentType,
            FileExtension = Path.GetExtension(form.File.FileName),
            SizeInBytes = form.File.Length,
            Content = memoryStream.ToArray()
        };

        return Ok(await requestHandler.Handle(request, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Download(
        int id,
        [FromServices] DownloadAttachmentRequestHandler requestHandler,
        CancellationToken cancellationToken)
    {
        var request = new DownloadAttachmentRequest { Id = id };
        var file = await requestHandler.Handle(request, cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(
        int id,
        [FromServices] DeactivateAttachmentRequestHandler requestHandler,
        CancellationToken cancellationToken)
    {
        var request = new DeactivateAttachmentRequest { Id = id };

        await requestHandler.Handle(request, cancellationToken);

        return NoContent();
    }
}

public class UploadAttachmentForm
{
    public string RequestType { get; set; } = null!;

    public int RequestId { get; set; }

    public string? Section { get; set; }

    public int? SectionEntityId { get; set; }

    public string DocumentType { get; set; } = null!;

    public string DocumentName { get; set; } = null!;

    public string? CustomDocumentName { get; set; }

    public IFormFile File { get; set; } = null!;
}
