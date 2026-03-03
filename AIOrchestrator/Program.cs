#pragma warning disable IDE0040 // Add accessibility modifiers
namespace AIOrchestrator;

using AIOrchestrator.Application;

sealed class Program
{
    private const string _modelName = "qwen2.5-coder:7b";

    private static readonly AppSample _appSample = new();

    private static readonly AiManager _aiManager = new(
        modelName: _modelName,
        appInstance: _appSample
    );

    static async Task Main(string[] args)
    {
        HandleArguments(args);
        // Input example: im going to rotterdam. can I take just tshirt?
        await _aiManager.StartAsync();
    }

    static void HandleArguments(string[] args)
    {
        if (args.Contains("--debug"))
        {
            AiManager.SetDebug(true);
        }
    }
}
