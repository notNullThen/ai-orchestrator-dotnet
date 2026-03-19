namespace AIOrchestrator.Core;

using AiAppFacade;
using OllamaClient;
using Types;
using Utilities;

public sealed class AiManager(string modelName, AiAppFacadeBase appInstance)
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
# SYSTEM
You are a function-calling engine.
You are FORBIDDEN from guessing, inventing, or using placeholders.
You are FORBIDDEN from responding with more than 1 function call.
Track the process in the Context History.
As soon as user request is fulfilled, call {nameof(appInstance.Exit)} function.

# Available Functions: {appInstance.GetDescription()}

# CONTEXT
User Input: ""{_userInput}""
History: {_contextHandler.GetContextJson()}

# CONSTRAINTS
{appInstance.GetConstraints()}

Avoid any explanations.
# YOUR RESPONSE SHOULD BE STRICTLY IN FORMAT:
{{
  ""Function"": ""string"",
  ""Parameters"": ""string[]""
}}
";

    public void SetDebug(bool debug) => Debug = debug;

    public async Task ConversationAsync()
    {
        if (_shouldExit)
        {
            return;
        }

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

    public async Task StartAsync(string userInput)
    {
        _userInput = userInput;
        appInstance.OnExit = Exit;
        await ConversationAsync();
    }

    private async Task<FunctionCall> GetFunctionAsync(string prompt)
    {
        var ollamaResponse = await _ollamaClient.RequestAsync(prompt: prompt, model: modelName);
        var response = ollamaResponse.Response;
        var functionJson = MarkdownProcess.RemoveCodeMarkdown(response);
        try
        {
            return MethodInvoker.Deserialize(functionJson);
        }
        catch (Exception exception)
        {
            throw new Exception(
                $"Failed to deserialize function call. Response was: {response}",
                exception
            );
        }
    }

    public void Exit()
    {
        Console.WriteLine($"\nOutput:\n{_aiOutput}");
        _shouldExit = true;
    }
}
