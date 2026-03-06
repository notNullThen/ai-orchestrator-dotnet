namespace AIOrchestrator.Core.Types;

using System.Text.Json.Serialization;

internal class FunctionCall
{
    [JsonPropertyOrder(1)]
    public required string Function { get; set; }

    [JsonPropertyOrder(2)]
    public string[] Parameters { get; set; } = [];
}
