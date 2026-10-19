using AiBusinessAssistant.DTOs.Chat;
using AiBusinessAssistant.Features.AI;
using Microsoft.AspNetCore.Mvc;

namespace AiBusinessAssistant.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class ChatController(IBusinessAssistantService assistant) : ControllerBase
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
}
