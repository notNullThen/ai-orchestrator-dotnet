#pragma warning disable IDE0040 // Add accessibility modifiers
namespace AIOrchestrator;

using Application;
using Core;

sealed class Program
{
    private const string ModelName = "qwen2.5-coder:7b";

    private static readonly AppSample _appSample = new();

    private static readonly AiManager _aiManager = new(
        modelName: ModelName,
        appInstance: _appSample
    );

    static async Task Main(string[] args)
    {
        HandleArguments(args);
        Console.WriteLine("Enter your input:");
        var userInput = Console.ReadLine()!;
        Console.WriteLine();
        await _aiManager.StartAsync(userInput);
    }

    static void HandleArguments(string[] args)
    {
        if (args.Contains("--debug"))
        {
            _aiManager.SetDebug(true);
        }
    }
}
