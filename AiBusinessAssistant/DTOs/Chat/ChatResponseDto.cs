namespace AiBusinessAssistant.DTOs.Chat;

public sealed class ChatResponseDto
{
    public int ConversationId { get; set; }
    public string UserMessage { get; set; } = string.Empty;
    public string AssistantMessage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public OrderWorkflowResponseDto? OrderWorkflow { get; set; }
    public OrderWorkflowResponseDto? PendingOrderWorkflow { get; set; }
}
