namespace AIOrchestrator.Core;

using System.Text.Json.Serialization;

public class FunctionCallResponse : FunctionCall
{
    [JsonPropertyOrder(3)]
    public required string Response { get; set; }
}
