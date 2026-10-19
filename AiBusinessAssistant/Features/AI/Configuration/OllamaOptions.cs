namespace AiBusinessAssistant.Features.AI.Configuration;

public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";
    public string Endpoint { get; set; } = "http://localhost:11434";
    public string ModelId { get; set; } = "phi3:mini";
    public int MaxHistoryMessages { get; set; } = 20;
    public int MaxMessageLength { get; set; } = 4000;
    public int RequestTimeoutSeconds { get; set; } = 90;
}
