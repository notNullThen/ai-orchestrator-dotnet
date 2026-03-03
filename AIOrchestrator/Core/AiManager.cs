namespace AIOrchestrator.Core;

using AIOrchestrator.OllamaClient;
using AIOrchestrator.Utilities;

public class AiManager(string modelName, AiFacadeBase appInstance)
{
    private bool _debug { get; set; }

    private string? _userInput;
    private string? _aiOutput;

    private readonly OllamaClient _ollamaClient = new();
    private readonly ContextHandler<FunctionCallResponse> _contextHandler = new();

    private string _managementPrompt =>
        @$"
# SYSTEM
You are a function-calling engine.
You are FORBIDDEN from guessing, inventing, or using placeholders.
Available Tools: {appInstance.GetDescription()}

# CONTEXT
User Input: ""{_userInput}""
History: {_contextHandler.GetContextJson()}

# CONSTRAINTS
1. If History already contains the answer, call {nameof(Exit)}().
2. Never repeat a function call found in History.
3. Correct grammar/casing in parameters.
4. Return ONLY raw JSON. No markdown, no backticks.

# RESPONSE FORMAT
{{
  ""Function"": ""string"",
  ""Parameters"": []
}}
";

    public void SetDebug(bool debug) => _debug = debug;

    public async Task ConversationAsync()
    {
        var function = await GetFunctionAsync(prompt: _managementPrompt);

        _aiOutput = (string)MethodInvoker.Execute(function, appInstance);

        var functionResponse = new FunctionCallResponse
        {
            Function = function.Function,
            Parameters = function.Parameters,
            Response = _aiOutput,
        };
        _contextHandler.AddToContext(functionResponse);
        if (_debug)
        {
            Console.WriteLine(_contextHandler.GetLastContextPartJson());
        }

        await ConversationAsync();
    }

    public async Task StartAsync()
    {
        appInstance.OnExit = Exit;
        Console.WriteLine("Enter your input:");
        _userInput = Console.ReadLine();
        Console.WriteLine();
        await ConversationAsync();
    }

    private async Task<FunctionCall> GetFunctionAsync(string prompt)
    {
        var response = await _ollamaClient.RequestAsync(prompt: prompt, model: modelName);
        var functionJson = MarkdownProcess.RemoveCodeMarkdown(response.Response);
        try
        {
            return MethodInvoker.Deserialize(functionJson);
        }
        catch (Exception exception)
        {
            throw new Exception(
                $"Failed to deserialize function call. Response was: {response.Response}",
                exception
            );
        }
    }

    public void Exit()
    {
        Console.WriteLine($"\nOutput:\n{_aiOutput}");
        Environment.Exit(0);
    }
}
