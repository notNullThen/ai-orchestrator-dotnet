#pragma warning disable IDE0040 // Add accessibility modifiers
namespace AIOrchestrator;

using AIOrchestrator.OllamaClient.Types;
using Application;
using Core;

sealed class Program
{
    private const string ModelName = "qwen2.5-coder:7b";

    private static readonly AppSample _appSample = new();

    private static readonly ApiRequestOptions _aiOptions = new()
    {
        Temperature = 0.7f,
        NumPredict = 100,
    };

    private static readonly AiManager _aiManager = new(
        modelName: ModelName,
        appInstance: _appSample,
        options: _aiOptions
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
