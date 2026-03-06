namespace AIOrchestrator.Core;

using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class ContextHandler<T>
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true,
    };

    private readonly List<T> _context = [];

    public IReadOnlyList<T> Context => _context.AsReadOnly();

    public void AddToContext(T part) => _context.Add(part);

    public string GetContextJson() => JsonSerializer.Serialize(_context, _jsonSerializerOptions);

    public string GetLastContextPartJson() =>
        JsonSerializer.Serialize(_context.LastOrDefault(), _jsonSerializerOptions);
}
