namespace AIOrchestrator.Support;

public class ConversationHandler
{
    public static string HistoryName { get; set; } = "chat history";
    private static string _task =>
        @$"
Your task is to respond to the user's input in a conversational manner continuing the {HistoryName} you have.
Don't use quotes in start and end of your response.
{HistoryName} is a JSON array of messages with defined roles.
Don't reply with {HistoryName} or JSON format.
Your responses should be **short and laconic**.
";

    private readonly ContextHandler<Message> _contextHandler = new();
    private string _conversationHistoryPrompt =>
        @$"
Your {HistoryName}:
{_contextHandler.GetContextJson()}";
    private readonly OllamaClient _ollamaClient = new();
    private string _model = "gemma3";
    private readonly List<string> _promptParts = [$"{Roles.System} message:"];
    private static readonly string _prefix = "   ";
    private static readonly string _roleSeparator = "\n";
    private static readonly string _messageSeparator = "\n\n";

    public ConversationHandler() => _promptParts.Add(_task);

    public static string Prefix => _prefix;

    public async Task ConversationAsync(string prompt) => await AIReplyAsync(prompt);

    public async Task ConversationAsync()
    {
        UserReply();

        _promptParts.Add(_conversationHistoryPrompt);

        var prompt = string.Join("\n", _promptParts);
        await AIReplyAsync(prompt);
        await ConversationAsync();
    }

    private void UserReply()
    {
        Console.Write($"{_roleSeparator}{_prefix}You:\n");
        var userInput = Console.ReadLine()!;

        _contextHandler.AddToContext(new() { Role = Roles.User, Content = userInput });
    }

    private async Task AIReplyAsync(string prompt)
    {
        await _ollamaClient.RequestStreamAsync(prompt, _model);
        var content = ConsoleStreamResponse();
        _contextHandler.AddToContext(new() { Role = Roles.Assistant, Content = content });
    }

    public void SetModel(string model) => _model = model;

    private string ConsoleStreamResponse()
    {
        Console.Write($"{_roleSeparator}{_prefix}ChatBot:\n");
        var content = string.Empty;

        ApiResponse line = new() { Done = false };
        while (!line.Done)
        {
            line = _ollamaClient.GetStreamApiResponse();
            var response = line.Response;

            content += response;
            Console.Write(response);
        }

        Console.Write($"{_messageSeparator}");
        return content;
    }
}
