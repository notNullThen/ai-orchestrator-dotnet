namespace AIOrchestrator.Support;

using System.Text.Json;
using System.Text.Json.Serialization;

public static class FancyJsonOptions
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };
}
