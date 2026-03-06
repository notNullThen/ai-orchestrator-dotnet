namespace AIOrchestrator.Core.AiAppFacade.Types;

public class FunctionDescription
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required List<FunctionParameter> Parameters { get; set; }
}
