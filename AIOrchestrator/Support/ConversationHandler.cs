namespace AIOrchestrator.Support;

using System.Text.Json;
using System.Text.Json.Serialization;

public class ConversationHandler
{
    private readonly OllamaClient _ollamaClient = new();
    private string _model = "mistral";
    private readonly List<Message> _conversation = [];
    private readonly List<string> _promptParts = [];
    private static readonly string _prefix = "   ";
    private static readonly string _roleSeparator = "\n";
    private static readonly string _messageSeparator = "\n\n";

    private string? _datasetContent;

    private static readonly JsonSerializerOptions _promptJsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public static string Prefix => _prefix;

    public async Task ConversationAsync(string prompt) => await AIReplyAsync(prompt);

    public async Task ConversationAsync()
    {
        var conversationJson = UserReply();

        /* PROMPT */

        // Beginning
        _promptParts.Add($"{Roles.System} message:");

        // Conversation history
        _promptParts.Add(@$"
You are {Roles.Assistant}.
Conversation history is a JSON array of messages with defined roles.
Don't reply with history of conversation.
Your conversation history:
{conversationJson}");

        // Dataset injection
        if (!string.IsNullOrWhiteSpace(_datasetContent))
        {
            _promptParts.Add($"Use this dataset: {_datasetContent}.");
        }

        // Task
        _promptParts.Add(@$"
Your task is to respond to the user's input in a conversational manner.
Your responses should be **very short and laconic**. Don't use quotes in start and end of your response.");

        var prompt = string.Join("\n", _promptParts);

        await AIReplyAsync(prompt);

        await ConversationAsync();
    }

    private string UserReply()
    {
        Console.Write($"{_roleSeparator}{_prefix}You:\n");
        var userInput = Console.ReadLine()!;

        _conversation.Add(new() { Role = Roles.User, Content = userInput });
        var conversationJson = JsonSerializer.Serialize(_conversation, _promptJsonSerializerOptions);

        return conversationJson;
    }


    private async Task AIReplyAsync(string prompt)
    {
        await _ollamaClient.RequestAsync(prompt, Roles.System, _model, stream: true);
        var content = ConsoleStreamResponse();
        _conversation.Add(new() { Role = Roles.Assistant, Content = content });
    }

    public void SetDatasetContent(string datasetContent) => _datasetContent = datasetContent;

    public void SetModel(string model) => _model = model;

    private string ConsoleStreamResponse()
    {
        Console.Write($"{_roleSeparator}{_prefix}ChatBot:\n");
        var content = string.Empty;

        ApiResponse line = new() { Done = false };
        while (!line.Done)
        {
            line = _ollamaClient.GetApiResponse();
            var response = line.Response;

            content += response;
            Console.Write(response);
        }

        Console.Write($"{_messageSeparator}");
        return content;
    }
}