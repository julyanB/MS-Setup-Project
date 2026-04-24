using CoreService.Application.Features.RequestMetaData.MarkRequestMetaDataSeen;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Web.Features;

[ApiController]
[Route("request-metadata")]
public class RequestMetaDataController : ControllerBase
{
    [HttpPatch("{requestType}/{id:int}/seen")]
    public async Task<IActionResult> MarkSeen(
        [AsParameters] MarkRequestMetaDataSeenRequest request,
        [FromServices] MarkRequestMetaDataSeenRequestHandler requestHandler,
        CancellationToken cancellationToken)
    {
        await requestHandler.Handle(request, cancellationToken);

        return NoContent();
    }
}
