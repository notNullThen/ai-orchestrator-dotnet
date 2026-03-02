namespace AIOrchestrator;

using AIOrchestrator.Support;
using AIOrchestrator.Support.OllamaClient;
using AIOrchestrator.Support.Types;
using AIOrchestrator.Weather;

public class AIManager(string modelName)
{
    public bool Debug { get; set; }

    private static string? _input;
    private static string? _output;

    private readonly OllamaClient _ollamaClient = new();
    private readonly ContextHandler<FunctionResponse> _contextHandler = new();

    private string _task =>
        @$"
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
        var function = await GetFunctionAsync(prompt: _task);

        _output = (string)MethodInvoker.Execute(function, new WeatherForecast());

        var functionResponse = new FunctionResponse
        {
            Function = function.Function,
            Parameters = function.Parameters,
            Response = _output,
        };

        _contextHandler.AddToContext(functionResponse);
        if (Debug)
        {
            Console.WriteLine(_contextHandler.GetLastContextPartJson());
        }

        await ConversationAsync();
    }

    public async Task StartAsync()
    {
        Console.WriteLine("Enter your input:");
        _input = Console.ReadLine();
        Console.WriteLine();
        await ConversationAsync();
    }

    private async Task<FunctionCall> GetFunctionAsync(string prompt)
    {
        var response = await _ollamaClient.RequestAsync(prompt: prompt, model: modelName);
        var functionJson = MarkdownProcess.RemoveCodeMarkdown(response.Response);
        return MethodInvoker.Deserialize(functionJson);
    }

    public static void Exit()
    {
        Console.WriteLine($"\nOutput:\n{_output}");
        Environment.Exit(0);
    }
}
