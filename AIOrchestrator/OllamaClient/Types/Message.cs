namespace AIOrchestrator.OllamaClient.Types;

using System.Text.Json.Serialization;

internal sealed class Message
{
    [JsonPropertyName("role")]
    public required Role Role { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }
}
