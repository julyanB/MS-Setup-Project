using CoreService.Application.Features.RequestMetaData.MarkRequestMetaDataSeen;
using CoreService.Application.Features.RequestMetaData.SearchRequestMetaData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Web.Features;

[ApiController]
[Route("request-metadata")]
public class RequestMetaDataController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<SearchRequestMetaDataResponse>> Search(
        [AsParameters] SearchRequestMetaDataRequest request,
        [FromServices] SearchRequestMetaDataRequestHandler requestHandler,
        CancellationToken cancellationToken)
    {
        var response = await requestHandler.Handle(request, cancellationToken);

        return Ok(response);
    }

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
