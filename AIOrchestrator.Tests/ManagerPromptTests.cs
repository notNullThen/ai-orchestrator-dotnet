namespace AIOrchestratorTests;

using AIOrchestrator.Application;
using AIOrchestrator.Core;
using AIOrchestrator.Core.Types;

public sealed class ManagerPromptTests
{
    private const string _modelName = "qwen2.5-coder:7b";

    [TestClass]
    public class ManagementPromptTests : AiTestsBase
    {
        protected override string ModelName => _modelName;
        protected override AppSample AppInstance => new();

        [TestMethod]
        public async Task ShouldManageProperlyIfNoDetailsInInputAsync()
        {
            SetLoopDetection(contextCountLimit: 3);

            var input = "will it be hot today";
            var context = AiManager.ContextHandler.Context;

            await AiManager.StartAsync(input);

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

            Assert.HasCount(3, context, "Might have called less functions than expected.");
        }
    }

    [TestClass]
    public class GrammarTests : AiTestsBase
    {
        protected override string ModelName => _modelName;
        protected override AppSample AppInstance => new();

        [TestMethod]
        public async Task ShouldCorrectParametersCasingAsync()
        {
            SetLoopDetection(contextCountLimit: 2);

            var input = "will it be hot today in paris";

            await AiManager.StartAsync(input);

            var functionCall = GetFirstFunctionCallByFunctionName(
                functionName: nameof(AppSample.GetWeather),
                AiManager
            );
            var parameter = functionCall.Parameters.First();

            Assert.AreEqual("Paris", parameter, ignoreCase: false);
        }

        [TestMethod]
        public async Task ShouldCorrectParametersTyposAsync()
        {
            SetLoopDetection(contextCountLimit: 2);

            var input = "what is the weather in buddappesst";

            await AiManager.StartAsync(input);

            var functionCall = GetFirstFunctionCallByFunctionName(
                functionName: nameof(AppSample.GetWeather),
                AiManager
            );
            var parameter = functionCall.Parameters.First();

            Assert.AreEqual("Budapest", parameter, ignoreCase: false);
        }

        [TestMethod]
        [Ignore("Unignore after fixing the issue with small typos correction.")]
        public async Task ShouldCorrectParametersSmallTyposAsync()
        {
            SetLoopDetection(contextCountLimit: 2);

            var input = "what is the weather in buddappesst";

            await AiManager.StartAsync(input);

            var functionCall = GetFirstFunctionCallByFunctionName(
                functionName: nameof(AppSample.GetWeather),
                AiManager
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
