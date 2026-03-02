namespace AIOrchestrator.Support.Types;

using System.Text.Json.Serialization;

public class FunctionCall
{
    [JsonPropertyOrder(1)]
    public required string Function { get; set; }

    [JsonPropertyOrder(2)]
    public object[] Parameters { get; set; } = [];
}
