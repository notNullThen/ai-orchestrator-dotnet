namespace AIOrchestrator.Support.Types;

using System.Text.Json.Serialization;

public class FunctionCallResponse : FunctionCall
{
    [JsonPropertyOrder(3)]
    public required string Response { get; set; }
}
