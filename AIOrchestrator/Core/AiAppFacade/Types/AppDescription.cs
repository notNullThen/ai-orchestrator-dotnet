namespace AIOrchestrator.Core.AiAppFacade.Types;

using System.Text.Json;

public class AppDescription : List<FunctionDescription>
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    public override string ToString() => JsonSerializer.Serialize(this, _jsonSerializerOptions);
}
