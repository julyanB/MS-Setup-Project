using EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.CreateBoardProposalRequest;
using EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.NextBoardProposalStep;
using EmployeeManagementService.Application.Features.BoardProposalRequests.Queries.SearchBoardProposalRequest;
using EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.AddBoardProposalAgendaItem;
using EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.AddBoardProposalTask;
using EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.AddBoardProposalVote;
using EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.ReorderBoardProposalTasks;
using EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.SetBoardProposalAgendaItemDecision;
using EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.UpdateBoardProposalTaskStatus;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementService.Web.Features;

[ApiController]
[Route("board-proposal-requests")]
public class BoardProposalRequestsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(
        [FromBody] CreateBoardProposalRequest request,
        [FromServices] CreateBoardProposalRequestHandler requestHandler,
        CancellationToken cancellationToken)
    {
        return Ok(await requestHandler.Handle(request, cancellationToken));
    }

    [HttpPost("{id:int}/next-step")]
    public async Task<IActionResult> NextStep(
        int id,
        [FromBody] NextBoardProposalStepRequest request,
        [FromServices] NextBoardProposalStepRequestHandler requestHandler,
        CancellationToken cancellationToken)
    {
        request.Id = id;
        await requestHandler.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/agenda-items")]
    public async Task<ActionResult<int>> AddAgendaItem(
        int id,
        [FromBody] AddBoardProposalAgendaItemRequest request,
        [FromServices] AddBoardProposalAgendaItemRequestHandler requestHandler,
        CancellationToken cancellationToken)
    {
        request.BoardProposalRequestId = id;
        return Ok(await requestHandler.Handle(request, cancellationToken));
    }

    [HttpPut("agenda-items/{agendaItemId:int}/decision")]
    public async Task<IActionResult> SetDecision(
        int agendaItemId,
        [FromBody] SetBoardProposalAgendaItemDecisionRequest request,
        [FromServices] SetBoardProposalAgendaItemDecisionRequestHandler requestHandler,
        CancellationToken cancellationToken)
    {
        request.AgendaItemId = agendaItemId;
        await requestHandler.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("agenda-items/{agendaItemId:int}/tasks")]
    public async Task<ActionResult<int>> AddTask(
        int agendaItemId,
        [FromBody] AddBoardProposalTaskRequest request,
        [FromServices] AddBoardProposalTaskRequestHandler requestHandler,
        CancellationToken cancellationToken)
    {
        request.AgendaItemId = agendaItemId;
        return Ok(await requestHandler.Handle(request, cancellationToken));
    }

    [HttpPost("agenda-items/{agendaItemId:int}/votes")]
    public async Task<ActionResult<int>> AddVote(
        int agendaItemId,
        [FromBody] AddBoardProposalVoteRequest request,
        [FromServices] AddBoardProposalVoteRequestHandler requestHandler,
        CancellationToken cancellationToken)
    {
        request.AgendaItemId = agendaItemId;
        return Ok(await requestHandler.Handle(request, cancellationToken));
    }

    [HttpPut("agenda-items/{agendaItemId:int}/tasks/reorder")]
    public async Task<IActionResult> ReorderTasks(
        int agendaItemId,
        [FromBody] ReorderBoardProposalTasksRequest request,
        [FromServices] ReorderBoardProposalTasksRequestHandler requestHandler,
        CancellationToken cancellationToken)
    {
        request.AgendaItemId = agendaItemId;
        await requestHandler.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpPut("tasks/{taskId:int}/status")]
    public async Task<IActionResult> UpdateTaskStatus(
        int taskId,
        [FromBody] UpdateBoardProposalTaskStatusRequest request,
        [FromServices] UpdateBoardProposalTaskStatusRequestHandler requestHandler,
        CancellationToken cancellationToken)
    {
        request.TaskId = taskId;
        await requestHandler.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:int}/search")]
    public async Task<ActionResult<BoardProposalRequestDetails>> Search(
        int id,
        [FromServices] SearchBoardProposalRequestHandler requestHandler,
        CancellationToken cancellationToken)
    {
        var request = new SearchBoardProposalRequest { Id = id };

        return Ok(await requestHandler.Handle(request, cancellationToken));
    }
}
