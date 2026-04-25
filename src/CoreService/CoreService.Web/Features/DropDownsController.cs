using CoreService.Application.Features.DropDowns;
using CoreService.Application.Features.DropDowns.GetDropDownOptions;
using CoreService.Application.Features.DropDowns.SetDropDownOptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Web.Features;

[ApiController]
[Route("drop-downs")]
public class DropDownsController : ControllerBase
{
    [HttpGet("{flow}/{key}")]
    public async Task<ActionResult<IReadOnlyCollection<DropDownOptionDetails>>> Get(
        [AsParameters] GetDropDownOptionsRequest request,
        [FromServices] GetDropDownOptionsRequestHandler requestHandler,
        CancellationToken cancellationToken)
    {
        return Ok(await requestHandler.Handle(request, cancellationToken));
    }

    [HttpPut("{flow}/{key}")]
    public async Task<IActionResult> Set(
        [AsParameters] SetDropDownOptionsRequest request,
        [FromServices] SetDropDownOptionsRequestHandler requestHandler,
        CancellationToken cancellationToken)
    {
        await requestHandler.Handle(request, cancellationToken);

        return NoContent();
    }
}
