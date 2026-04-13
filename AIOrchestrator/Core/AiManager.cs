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
SYSTEM:
You are a function calling engine.

YOU MUST FOLLOW ALL RULES STRICTLY.

OUTPUT RULES:
- Return ONLY valid JSON.
- DO NOT write any text.
- DO NOT explain.
- DO NOT use markdown or backticks.
- Call EXACTLY ONE function.

STRICT JSON STRUCTURE:
- The response MUST be a SINGLE valid JSON object.

FORMAT:
{{
  ""Function"": ""string"",
  ""Parameters"": [""string""]
}}

FUNCTION CALL RULES:
- You MUST call EXACTLY ONE function.
- NEVER respond with more than ONE function.
- NEVER return multiple JSON objects.
- NEVER simulate multiple steps.

PARAMETERS RULES:
- Parameters MUST contain ONLY raw values.
- Each parameter MUST be a string value.
- NEVER include parameter names.

BEHAVIOR:
- You operate step-by-step.
- Each response = ONE step = ONE function call.
- After each call, you will be called with updated History.
- NEVER try to complete the full task in one response.

FUNCTIONS:
{appInstance.GetDescription()}

STATE:
User: {_userInput}
History: {_contextHandler.GetContextJson()}

CONSTRAINTS:
{appInstance.GetConstraints()}

IMPORTANT:
- ONLY use functions from the FUNCTIONS list.
- If data already exists in History -> DO NOT call function again.
- If task is complete -> you MUST call {nameof(appInstance.Exit)}.
";

    public string GetManagementPrompt() => ManagementPrompt;

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
