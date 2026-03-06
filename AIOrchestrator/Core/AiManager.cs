namespace AIOrchestrator.Core;

using AIOrchestrator.Core.AiAppFacade;
using AIOrchestrator.Core.Types;
using AIOrchestrator.OllamaClient;
using AIOrchestrator.Utilities;

internal sealed class AiManager(string modelName, AiAppFacadeBase appInstance)
{
    private bool _debug { get; set; }

    private string? _userInput;
    private string? _aiOutput;
    private bool _shouldExit;

    private readonly OllamaClient _ollamaClient = new();
    public readonly ContextHandler<FunctionCallResponse> ContextHandler = new();

    private string _managementPrompt =>
        @$"
# SYSTEM
You are a function-calling engine.
You are FORBIDDEN from guessing, inventing, or using placeholders.
Available Tools: {appInstance.GetDescription()}

# CONTEXT
User Input: ""{_userInput}""
History: {ContextHandler.GetContextJson()}

# CONSTRAINTS
1. If History already contains the answer, call {nameof(Exit)}().
2. Never repeat a function call found in History.
3. Correct grammar/casing in parameters.
4. Return ONLY raw JSON. No markdown, no backticks.

# RESPONSE FORMAT
{{
  ""Function"": ""string"",
  ""Parameters"": ""string[]""
}}
";

    public void SetDebug(bool debug) => _debug = debug;

    public async Task ConversationAsync()
    {
        if (_shouldExit)
        {
            return;
        }

        var function = await GetFunctionAsync(prompt: _managementPrompt);

        _aiOutput = (string)MethodInvoker.Execute(function, appInstance);

        var functionResponse = new FunctionCallResponse
        {
            Function = function.Function,
            Parameters = function.Parameters,
            Response = _aiOutput,
        };
        ContextHandler.AddToContext(functionResponse);
        if (_debug)
        {
            Console.WriteLine(ContextHandler.GetLastContextPartJson());
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
