using AiBusinessAssistant.DTOs.Chat;
using AiBusinessAssistant.Features.AI;
using Microsoft.AspNetCore.Mvc;

namespace AiBusinessAssistant.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class ChatController(IBusinessAssistantService assistant, IOrderWorkflowService orderWorkflow) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ChatResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChatResponseDto>> SendMessage(
        [FromBody] SendChatRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await assistant.SendMessageAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{conversationId:int}/order-workflow")]
    [ProducesResponseType(typeof(OrderWorkflowResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderWorkflowResponseDto>> GetOrderWorkflow(int conversationId, CancellationToken cancellationToken)
    {
        var workflow = await orderWorkflow.GetActiveAsync(conversationId, cancellationToken);
        return workflow is null ? NoContent() : Ok(workflow);
    }

    [HttpPost("{conversationId:int}/order-workflow")]
    [ProducesResponseType(typeof(OrderWorkflowResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderWorkflowResponseDto>> OrderWorkflowAction(
        int conversationId,
        [FromBody] OrderWorkflowActionRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await orderWorkflow.HandleActionAsync(conversationId, request, cancellationToken);
        return Ok(response);
    }
}
