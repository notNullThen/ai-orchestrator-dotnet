namespace AIOrchestrator.Core;

using System.Text.Json.Serialization;

internal sealed class FunctionCallResponse : FunctionCall
{
    [JsonPropertyOrder(3)]
    public required string Response { get; set; }
}
