namespace AIOrchestratorTests;

using AIOrchestrator.Application;
using AIOrchestrator.Core;
using AIOrchestrator.Core.Types;

public sealed class ManagerPromptTests
{
    private const string _modelName = "qwen2.5-coder:7b";

    private static readonly AppSample _appSample = new();

    [TestClass]
    public class ManagementPromptTests
    {
        [TestMethod]
        public async Task ShouldManageProperlyIfNoDetailsInInputAsync()
        {
            var aiManager = new AiManager(modelName: _modelName, appInstance: _appSample);
            var input = "will it be hot today";
            var context = aiManager.ContextHandler.Context;

            await aiManager.StartAsync(input);

            var locationCall = context[0];
            var weatherCall = context[1];

            Assert.AreEqual(
                nameof(AppSample.GetLocation),
                locationCall.Function,
                $"Should call the {nameof(AppSample.GetLocation)}() function first."
            );
            Assert.IsEmpty(
                locationCall.Parameters,
                $"Halucinated parameters for the {nameof(AppSample.GetLocation)}() function."
            );

            Assert.AreEqual(
                nameof(AppSample.GetWeather),
                weatherCall.Function,
                $"Should call the {nameof(AppSample.GetWeather)}() function."
            );
            Assert.AreEqual(
                locationCall.Response,
                weatherCall.Parameters.First(),
                $"Should use the location from the first call as parameter for the {nameof(AppSample.GetWeather)}() function."
            );

            Assert.HasCount(3, context, "Might have called more functions than expected.");
        }
    }

    [TestClass]
    public class GrammarTests
    {
        [TestMethod]
        public async Task ShouldCorrectParametersCasingAsync()
        {
            var aiManager = new AiManager(modelName: _modelName, appInstance: _appSample);
            var input = "will it be hot today in paris";
            var context = aiManager.ContextHandler.Context;

            await aiManager.StartAsync(input);

            var functionCall = GetFirstFunctionCallByFunctionName(
                functionName: nameof(AppSample.GetWeather),
                aiManager
            );
            var parameter = functionCall.Parameters.First();

            Assert.AreEqual("Paris", parameter, ignoreCase: false);
        }

        [TestMethod]
        public async Task ShouldCorrectParametersTyposAsync()
        {
            var aiManager = new AiManager(modelName: _modelName, appInstance: _appSample);
            var input = "what is the weather in buddappesst";
            var context = aiManager.ContextHandler.Context;

            await aiManager.StartAsync(input);

            var functionCall = GetFirstFunctionCallByFunctionName(
                functionName: nameof(AppSample.GetWeather),
                aiManager
            );
            var parameter = functionCall.Parameters.First();

            Assert.AreEqual("Budapest", parameter, ignoreCase: false);
        }

        [TestMethod]
        [Ignore("Unignore after fixing the issue with small typos correction.")]
        public async Task ShouldCorrectParametersSmallTyposAsync()
        {
            var aiManager = new AiManager(modelName: _modelName, appInstance: _appSample);
            var input = "what is the weather in buddappesst";
            var context = aiManager.ContextHandler.Context;

            await aiManager.StartAsync(input);

            var functionCall = GetFirstFunctionCallByFunctionName(
                functionName: nameof(AppSample.GetWeather),
                aiManager
            );
            var parameter = functionCall.Parameters.First();

            Assert.AreEqual("Budapest", parameter, ignoreCase: false);
        }
    }

    private static FunctionCallResponse GetFirstFunctionCallByFunctionName(
        string functionName,
        AiManager aiManager
    ) => aiManager.ContextHandler.Context.First(param => param.Function == functionName);
}
