namespace AiBusinessAssistant.DTOs.Chat;

public sealed class SendChatRequestDto
{
    public int? ConversationId { get; set; }
    public string Message { get; set; } = string.Empty;
}
