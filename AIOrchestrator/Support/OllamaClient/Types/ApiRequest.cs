namespace AIOrchestrator.Support.OllamaClient.Types;

using System.Text.Json.Serialization;

public class ApiRequest
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
}
