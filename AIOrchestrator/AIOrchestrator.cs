namespace AIOrchestrator;

using AIOrchestrator.Support;
using AIOrchestrator.Weather;

public class AIManager
{
    public bool Debug { get; set; }
    private const string _model = "gemma3";

    private static string _input = string.Empty;
    private static string _output = string.Empty;

    private readonly OllamaClient _ollamaClient = new();
    private readonly ContextHandler<MethodInvoker.FunctionResponse> _contextHandler = new();

    private string _task => @$"
User's Input: ""{_input}""

**Available Functions and Parameters**:
1. GetWeather(location):
    - Description: Returns a weather forecast for the given location.
    - Parameter: 
        - location (string): The location for which to retrieve the weather forecast.

2. GetLocation():
    - Description: Retrieves the current location.
    - Parameters: None.

3. Exit():
    - Description: Terminates the program.
    - Parameters: None.

**Functions Call History**:
{_contextHandler.GetContextJson()}

**Instructions**:
1. Analyze the User's Input and the Functions Call History to understand the context.
2. Check the User's Input for explicit mentions of information relevant to the available functions. If the required information is already provided in the User's Input (e.g. location is already mentioned), do not call a function to retrieve it. Use information from the User's Input directly.
3. Determine which Function to call based on the information required to fulfill the User's Input.
4. If some function is shown in the Functions Call History, **do not call this function**. Instead, use the information from that function response.
5. If you don't have enough information to fulfill the User's Input, call the function you need to make this information appear in Functions Call History.
6. If the most recent response satisfies the User's Input, call the **Exit()** Function to conclude the conversation.

**Response Format**:
Return a single Function call in JSON format, as shown below:
{{
    ""Function"": ""FunctionName"",
    ""Parameters"": [""Parameter1"", ""Parameter2""]
}}

- The response must consist of only one Function.
- Do **not** include arrays of Functions.
- Provide **only** the JSON body—exclude any explanations or additional text.
";

    public async Task ConversationAsync()
    {
        var instructionJsonResponse = await RequestAIAsync(prompt: _task);

        var instructionJson = MarkdownProcess.RemoveCodeMarkdown(instructionJsonResponse);

        _output = (string)MethodInvoker.Execute(instructionJson, new WeatherForecast())!;

        var functionCall = MethodInvoker.Deserialize(instructionJson);
        var functionResponse = new MethodInvoker.FunctionResponse
        {
            Function = functionCall.Function,
            Parameters = functionCall.Parameters,
            Response = _output
        };

        _contextHandler.AddToContext(functionResponse);
        if (Debug)
        {
            Console.WriteLine(_contextHandler.GetLastContextPartJson());
        }

        await ConversationAsync();
    }

    public async Task StartAsync() // Delete
    {
        Console.WriteLine("Enter your input:");
        _input = Console.ReadLine()!;
        Console.WriteLine();
        // _input = "im going to rotterdam. can I take just tshirt?";
        await ConversationAsync();
    }

    private async Task<string> RequestAIAsync(string prompt)
    {
        var response = await _ollamaClient.RequestAsync(prompt: prompt, model: _model);
        return response.Response;
    }

    public static void Exit()
    {
        Console.WriteLine($"\nOutput:\n{_output}");
        Environment.Exit(0);
    }
}