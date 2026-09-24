namespace AIOrchestrator.Core;

using AiAppFacade;
using OllamaClient;
using OllamaClient.Types;

internal sealed class ReflectiveRecovery(
    string constraintsFilePath,
    AiAppFacadeBase appInstance,
    OllamaClient ollamaClient,
    string modelName,
    ApiRequestOptions? options
)
{
    private readonly string _path = Path.GetFullPath(constraintsFilePath);
    private string _constraints = string.Empty;

    public string Constraints => _constraints;

    public async Task LoadAsync(CancellationToken cancellationToken) =>
        _constraints = File.Exists(_path)
            ? await File.ReadAllTextAsync(_path, cancellationToken)
            : string.Empty;

    public async Task<string> HealAsync(
        Exception error,
        string? userInput,
        string? latestModelOutput,
        string contextJson,
        Action<string>? onPromptPrepared,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        await LoadAsync(cancellationToken);
        var errorDetails = error.InnerException is null
            ? error.Message
            : $"{error.Message}\nCause: {error.GetBaseException().Message}";

        var prompt = $$"""
            SYSTEM:
            Understand the user's task and story, successful calls, available functions,
            instructions, constraints, latest model output, failed call, and error.
            Return one concise NEW constraint that would prevent this wrong output or error
            from happening again. If an existing constraint already covers it, or the cause
            is unclear, return only this JSON call instead:
            {"Function":"Exit","Parameters":[]}
            Otherwise return only the constraint as plain text, without explanation or Markdown.

            USER TASK:
            {{userInput}}

            HISTORY / STORY:
            {{contextJson}}

            FUNCTIONS:
            {{appInstance.GetDescription()}}

            APPLICATION INSTRUCTIONS AND CONSTRAINTS:
            {{appInstance.GetConstraints()}}

            EXISTING LEARNED CONSTRAINTS FROM FILE:
            {{_constraints}}

            LATEST MODEL OUTPUT:
            {{latestModelOutput}}

            FAILED CALL AND ERROR:
            {{errorDetails}}
            """;

        onPromptPrepared?.Invoke(prompt);

        var response = await ollamaClient.RequestAsync(
            prompt,
            modelName,
            options: options,
            cancellationToken: cancellationToken
        );

        var answer = response.Response.Trim();

        var isExitCall = FunctionsDeserializer
            .Deserialize(answer)
            .Any(call => call?.Function == nameof(appInstance.Exit));

        if (answer.Length == 0 || isExitCall || IsExistingConstraint(answer))
        {
            appInstance.Exit();
            return response.Response;
        }

        await CreateConstraintsFileAsync(cancellationToken);

        await File.AppendAllTextAsync(_path, answer + "\n", cancellationToken);
        _constraints += answer + "\n";

        return response.Response;
    }

    private bool IsExistingConstraint(string answer)
    {
        var proposed = answer.Trim().Trim('"');
        return _constraints
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(line =>
                string.Equals(line.Trim('"'), proposed, StringComparison.OrdinalIgnoreCase)
            );
    }

    private async Task CreateConstraintsFileAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        if (!File.Exists(_path))
        {
            _constraints = "# Learned constraints\n\n";
            await File.WriteAllTextAsync(_path, _constraints, cancellationToken);
        }
    }
}
