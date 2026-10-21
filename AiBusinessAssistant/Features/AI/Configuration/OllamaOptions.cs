namespace AiBusinessAssistant.Features.AI.Configuration;

public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";
    public string Endpoint { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public int MaxHistoryMessages { get; set; } = 20;
    public int MaxMessageLength { get; set; } = 4000;
    public int RequestTimeoutSeconds { get; set; } = 90;
}
