namespace AIOrchestrator.Support;

using System.Text.Json;

public class ContextHandler<T>
{
    private readonly List<T> _context = [];

    public void AddToContext(T part) => _context.Add(part);

    public string GetContextJson() =>
        JsonSerializer.Serialize(_context, FancyJsonOptions.SerializerOptions);

    public string GetLastContextPartJson() =>
        JsonSerializer.Serialize(_context.LastOrDefault(), FancyJsonOptions.SerializerOptions);
}
