namespace AIOrchestrator.Core;

using System.Text.Json;
using AiAppFacade;
using OllamaClient;
using OllamaClient.Types;
using Types;

public sealed class AiManager(
    string modelName,
    AiAppFacadeBase appInstance,
    ApiRequestOptions? options = null,
    string? ollamaBaseUrl = null,
    TimeSpan? ollamaHttpTimeout = null
)
{
    /// <summary>Set a Markdown file path to enable recovery from invalid model calls.</summary>
    public string? HealingConstraintsFilePath { get; init; }

    public ContextHandler<FunctionCallResponse> ContextHandler => _contextHandler;

    public ErrorHandler ErrorHandler =>
        _errorHandlerField ??= new ErrorHandler(modelName, _contextHandler);

    // Store ErrorHandler in a private field so it is not recreated on every class initiation and the error information is not lost.
    private ErrorHandler? _errorHandlerField;
    private MethodInvoker? _methodInvokerField;
    private MethodInvoker _methodInvoker => _methodInvokerField ??= new MethodInvoker();

    private string? _userInput;
    private string? _latestModelOutput;
    private bool _shouldExit;
    private ReflectiveRecovery? _reflectiveRecovery;

    private readonly OllamaClient _ollamaClient = new(ollamaBaseUrl, ollamaHttpTimeout);

    private readonly ContextHandler<FunctionCallResponse> _contextHandler = new();

    private ReflectiveRecovery? Recovery
    {
        get
        {
            if (HealingConstraintsFilePath is null)
            {
                return null;
            }

            _reflectiveRecovery ??= new ReflectiveRecovery(
                HealingConstraintsFilePath,
                appInstance,
                _ollamaClient,
                modelName,
                options
            );

            return _reflectiveRecovery;
        }
    }

    private string ManagementPrompt =>
        @$"
SYSTEM:
You are a strict JSON function calling engine. You must output {(appInstance.MultipleFunctionsAtOneResponse ? "one or more JSON objects." : "exactly one JSON object")} and nothing else.

{(appInstance.MultipleFunctionsAtOneResponse ? "Pack all required function calls into one response. Output each function call as a separate JSON object." : "One response is only one function call. Don't output multiple function calls.")}

Strictly adhere to the following JSON format.
{{
  ""Function"": ""FunctionName"",
  ""Parameters"": [""value"", ""another-value"", ...]
}}

Strictly follow these JSON format types:
- ""Function"": string
- ""Parameters"": string[] (dont put parameter names, put only values in the array)

RULES:
1. Return only {(appInstance.MultipleFunctionsAtOneResponse ? "one or more JSON objects" : "a single JSON object")}.
2. Don't wrap the JSON in Markdown formatting, backticks, or write any text explanations.
3. {(appInstance.MultipleFunctionsAtOneResponse ? "Call all functions required to fulfill the request in the same response." : "Call exactly one function per response.")}
4. Before choosing a function, compare the User request with History and decide whether the requested result is already satisfied.
5. Call `{nameof(appInstance.Exit)}` function if user's request is fulfilled.
6. {(appInstance.MultipleFunctionsAtOneResponse ? "Plan all required function calls and output them together in one response." : "Operate step-by-step.")}
7. Evaluate History before deciding the next step.

FUNCTIONS:
{appInstance.GetDescription()}

CONSTRAINTS:
{appInstance.GetConstraints()}
{Recovery?.Constraints}

STATE:
User: {_userInput}
History: {_contextHandler.GetContextJson()}

{(appInstance.MultipleFunctionsAtOneResponse ? "You MUST process the STATE and reply with one or more JSON objects. If the STATE already satisfies the User request, the response MUST call" : "You MUST process the STATE and reply with EXACTLY ONE JSON function call. If the STATE already satisfies the User request, that call MUST be")} {nameof(appInstance.Exit)}.
";

    public string GetManagementPrompt() => ManagementPrompt;

    public async Task ConversationAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_shouldExit)
        {
            return;
        }

        try
        {
            if (Recovery is not null)
            {
                await Recovery.LoadAsync(cancellationToken);
            }

            while (!_shouldExit)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ProcessStepAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _shouldExit = true;
            throw;
        }
        catch (Exception ex)
        {
            _shouldExit = true;
            throw new Exception(
                ErrorHandler.GetFullErrorMessage(
                    $"An error occurred during AI conversation execution.\nError: {ex.Message}"
                ),
                ex
            );
        }
    }

    private async Task ProcessStepAsync(CancellationToken cancellationToken)
    {
        try
        {
            _latestModelOutput = null;
            var functions = await RequestFunctionAsync(ManagementPrompt, cancellationToken);
            if (Recovery is not null && functions.Count == 0)
            {
                throw new JsonException("The model returned no function call.");
            }

            foreach (var function in functions)
            {
                if (function is not null)
                {
                    ExecuteFunction(function);
                }
            }
        }
        catch (Exception exception)
            when (Recovery is not null && exception is not OperationCanceledException)
        {
            var result = await Recovery.HealAsync(
                exception,
                _latestModelOutput,
                _contextHandler.GetContextJson(),
                cancellationToken
            );
            ErrorHandler.SetLatestAiOutput(result);
        }
    }

    private void ExecuteFunction(FunctionCall function)
    {
        var functionResult = _methodInvoker.Execute(function, appInstance);
        _contextHandler.AddToContext(
            new FunctionCallResponse
            {
                Function = function.Function,
                Parameters = function.Parameters,
                Response = functionResult,
            }
        );
    }

    public async Task StartAsync(string userInput, CancellationToken cancellationToken = default)
    {
        _userInput = userInput;
        _shouldExit = false;
        ErrorHandler.SetUserInput(userInput);

        appInstance.OnExit = Exit;
        await ConversationAsync(cancellationToken);
    }

    private async Task<List<FunctionCall?>> RequestFunctionAsync(
        string prompt,
        CancellationToken cancellationToken = default
    )
    {
        var apiOptions =
            options == null
                ? null
                : new ApiRequestOptions
                {
                    Temperature = options.Temperature,
                    NumPredict = options.NumPredict,
                };

        var ollamaResponse = await _ollamaClient.RequestAsync(
            prompt: prompt,
            model: modelName,
            options: apiOptions,
            cancellationToken: cancellationToken
        );

        var response = ollamaResponse.Response;

        _latestModelOutput = response;
        ErrorHandler.SetLatestAiOutput(response);

        return FunctionsDeserializer.Deserialize(response);
    }

    public void Exit() => _shouldExit = true;
}
