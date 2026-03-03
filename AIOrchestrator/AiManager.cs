namespace AIOrchestrator;

using AIOrchestrator.Application;
using AIOrchestrator.Support;
using AIOrchestrator.Support.OllamaClient;
using AIOrchestrator.Support.Types;

public class AiManager(string modelName)
{
    private static bool _debug { get; set; }

    private static string? _userInput;
    private static string? _aiOutput;

    private readonly OllamaClient _ollamaClient = new();
    private readonly ContextHandler<FunctionCallResponse> _contextHandler = new();
    private readonly AppSample _appSample = new();

    private string _task =>
        @$"
# SYSTEM
You are a function-calling engine. 
Available Tools: {_appSample.GetDescription()}

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

    public static void SetDebug(bool debug) => _debug = debug;

    public async Task ConversationAsync()
    {
        var function = await GetFunctionAsync(prompt: _task);

        _aiOutput = (string)MethodInvoker.Execute(function, _appSample);

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

    public static void Exit()
    {
        Console.WriteLine($"\nOutput:\n{_aiOutput}");
        Environment.Exit(0);
    }
}
