namespace AIOrchestrator.Support;

using System.Text.Json;
using System.Text.Json.Serialization;

public class ContextHandler<T>
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly List<T> _context = [];

    public void AddToContext(T part) => _context.Add(part!);
    public string GetContextJson() => JsonSerializer.Serialize(_context, _jsonSerializerOptions);
    public string GetLastContextPartJson() => JsonSerializer.Serialize(_context.LastOrDefault(), _jsonSerializerOptions);
}