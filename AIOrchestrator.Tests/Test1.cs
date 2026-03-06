namespace AIOrchestratorTests;

using System.IO;
using AIOrchestrator.Application;
using AIOrchestrator.Core;

[TestClass]
public sealed class Test1
{
    private const string _modelName = "qwen2.5-coder:7b";

    private static readonly AppSample _appSample = new();

    private static readonly AiManager _aiManager = new(
        modelName: _modelName,
        appInstance: _appSample
    );

    [TestMethod]
    // [Timeout(5000)] // Timeout after 5 seconds to prevent hanging
    public async Task ShouldReturnWeatherForDubaiAsync()
    {
        var input = "get weather for budapest";

        await _aiManager.StartAsync(input);
        var output = _aiManager.ContextHandler.Context;
    }
}
