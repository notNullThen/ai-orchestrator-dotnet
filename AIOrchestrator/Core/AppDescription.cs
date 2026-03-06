namespace AIOrchestrator.Core.AiFacade;

using System.Text.Json;

public class AppDescription : List<FunctionDescription>
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    public override string ToString() => JsonSerializer.Serialize(this, _jsonSerializerOptions);
}
