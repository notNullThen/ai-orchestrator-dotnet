namespace AIOrchestrator.Core;

using AiAppFacade;
using OllamaClient;
using OllamaClient.Types;
using Types;
using Utilities;

public sealed class AiManager(
    string modelName,
    AiAppFacadeBase appInstance,
    ApiRequestOptions? options = null
)
{
    public ContextHandler<FunctionCallResponse> ContextHandler => _contextHandler;

    private bool Debug { get; set; }

    private string? _userInput;
    private object? _aiOutput;
    private bool _shouldExit;

    private readonly OllamaClient _ollamaClient = new();
    private readonly ContextHandler<FunctionCallResponse> _contextHandler = new();

    private string ManagementPrompt =>
        @$"
SYSTEM:
You are a strict JSON function calling engine. You must output EXACTLY ONE JSON object and NOTHING else.

YOU MUST strictly adhere to the following JSON format.
{{
  ""Function"": ""FunctionName"",
  ""Parameters"": [""value1"", ""value2""]
}}

RULES:
1. You MUST return ONLY a single JSON object.
2. You MUST NOT wrap the JSON in Markdown formatting, backticks, or write any text explanations.
3. You MUST call EXACTLY ONE function per response.
4. You MUST use ONLY functions from the FUNCTIONS list.
5. If the task is fully completed, you MUST call {nameof(appInstance.Exit)}.
6. You MUST operate step-by-step.
7. You MUST evaluate History before deciding the next step.

FUNCTIONS:
{appInstance.GetDescription()}

CONSTRAINTS:
{appInstance.GetConstraints()}

STATE:
User: {_userInput}
History: {_contextHandler.GetContextJson()}

You MUST process the STATE and reply with EXACTLY ONE JSON function call.
";

    public string GetManagementPrompt() => ManagementPrompt;

    public void SetDebug(bool debug) => Debug = debug;

    public async Task ConversationAsync()
    {
        if (_shouldExit)
        {
            return;
        }

        try
        {
            var function = await GetFunctionAsync(prompt: ManagementPrompt);

            _aiOutput = MethodInvoker.Execute(function, appInstance);

            var functionResponse = new FunctionCallResponse
            {
                Function = function.Function,
                Parameters = function.Parameters,
                Response = _aiOutput,
            };
            _contextHandler.AddToContext(functionResponse);
            if (Debug)
            {
                Console.WriteLine(_contextHandler.GetLastContextPartJson());
            }

            await ConversationAsync();
        }
        catch (Exception exception)
        {
            Console.WriteLine($"[AiManager] Error during conversation: {exception.Message}");
            _shouldExit = true;
            throw; // Re-throw to inform the caller, but now we've set _shouldExit
        }
    }

    public async Task StartAsync(string userInput)
    {
        _userInput = userInput;
        _shouldExit = false;
        appInstance.OnExit = Exit;
        await ConversationAsync();
    }

    private async Task<FunctionCall> GetFunctionAsync(string prompt)
    {
        var apiOptions =
            options == null
                ? null
                : new ApiRequestOptions
                {
                    Temperature = options.Temperature,
                    NumPredict = options.NumPredict,
                };

        try
        {
            var ollamaResponse = await _ollamaClient.RequestAsync(
                prompt: prompt,
                model: modelName,
                options: apiOptions
            );

            var response = ollamaResponse.Response;
            var functionJson = MarkdownProcess.RemoveCodeMarkdown(response);

            return MethodInvoker.Deserialize(functionJson);
        }
        catch (Exception exception)
        {
            Console.WriteLine("[AiManager] ERROR decoding AI response:");
            Console.WriteLine($"Exception: {exception.Message}");

            if (exception.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {exception.InnerException.Message}");
            }

            throw;
        }
    }

    public void Exit()
    {
        Console.WriteLine($"\nOutput:\n{_aiOutput}");
        _shouldExit = true;
    }
}
