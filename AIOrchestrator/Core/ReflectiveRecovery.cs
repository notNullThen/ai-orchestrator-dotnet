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
        string? latestModelOutput,
        string contextJson,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var prompt = $$"""
            SYSTEM:
            Analyze the error. If you can identify a way to prevent it, return one concise constraint.
            Base the constraint on the available functions and the error.
            Return only the constraint as plain text, without Markdown, JSON, or explanations.
            If there is no clue why the error happened, instead return only this JSON function call:
            {"Function":"Exit","Parameters":[]}

            FUNCTIONS:
            {{appInstance.GetDescription()}}

            CURRENT CONSTRAINTS:
            {{appInstance.GetConstraints()}}
            {{_constraints}}

            LATEST MODEL OUTPUT:
            {{latestModelOutput}}

            HISTORY:
            {{contextJson}}

            ERROR:
            {{error.InnerException ?? error}}
            """;

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

        if (answer.Length == 0 || isExitCall)
        {
            appInstance.Exit();
            return response.Response;
        }

        await CreateConstraintsFileAsync(cancellationToken);

        await File.AppendAllTextAsync(_path, answer + "\n", cancellationToken);
        _constraints += answer + "\n";

        return response.Response;
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
