using EmployeeManagementService.Application.Features.Identity;
using EmployeeManagementService.Application.Features.Identity.Queries.GetEmployees;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementService.Web.Features;

[ApiController]
[Route("employees")]
public class EmployeesController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmployeeLookupItem>>> GetEmployees(
        [FromQuery] GetEmployeesRequest request,
        [FromServices] GetEmployeesRequestHandler requestHandler,
        CancellationToken cancellationToken)
        => Ok(await requestHandler.Handle(request, cancellationToken));
}
