namespace AIOrchestrator.Support.Types;

using System.Text.Json.Serialization;

public class FunctionResponse : FunctionCall
{
    [JsonPropertyOrder(3)]
    public required string Response { get; set; }
}
