#pragma warning disable IDE0040 // Add accessibility modifiers
namespace AIOrchestrator;

sealed class Program
{
    private const string _modelName = "qwen2.5:7b";

    private static readonly AIManager _aiManager = new(modelName: _modelName);

    static async Task Main(string[] args)
    {
        HandleArguments(args);
        await _aiManager.StartAsync();
    }

    static void HandleArguments(string[] args)
    {
        if (args.Contains("--debug"))
        {
            _aiManager.Debug = true;
        }
    }
}
