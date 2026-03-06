namespace AIOrchestratorTests;

using AIOrchestrator.Application;
using AIOrchestrator.Core;
using AIOrchestrator.Core.Types;

[TestClass]
public sealed class ManagerPromptTests
{
    private const string _modelName = "qwen2.5-coder:7b";

    private static readonly AppSample _appSample = new();

    private static readonly AiManager _aiManager = new(
        modelName: _modelName,
        appInstance: _appSample
    );

    [TestMethod]
    public async Task ShouldCorrectParametersCasingAsync()
    {
        var input = "will it be hot today in paris";
        var context = _aiManager.ContextHandler.Context;

        await _aiManager.StartAsync(input);

        var functionCall = GetFirstFunctionCallByName(nameof(AppSample.GetWeather));
        var parameter = functionCall.Parameters.First();

        Assert.AreEqual("Paris", parameter, ignoreCase: false);
    }

    [TestMethod]
    public async Task ShouldCorrectParametersTyposAsync()
    {
        var input = "what is the weather in buddappesst";
        var context = _aiManager.ContextHandler.Context;

        await _aiManager.StartAsync(input);

        var functionCall = GetFirstFunctionCallByName(nameof(AppSample.GetWeather));
        var parameter = functionCall.Parameters.First();

        Assert.AreEqual("Budapest", parameter, ignoreCase: false);
    }

    [TestMethod]
    [Ignore("Unignore after fixing the issue with small typos correction.")]
    public async Task ShouldCorrectParametersSmallTyposAsync()
    {
        var input = "what is the weather in buddappesst";
        var context = _aiManager.ContextHandler.Context;

        await _aiManager.StartAsync(input);

        var functionCall = GetFirstFunctionCallByName(nameof(AppSample.GetWeather));
        var parameter = functionCall.Parameters.First();

        Assert.AreEqual("Budapest", parameter, ignoreCase: false);
    }

    private static FunctionCallResponse GetFirstFunctionCallByName(string functionName) =>
        _aiManager.ContextHandler.Context.First(param => param.Function == functionName);
}
