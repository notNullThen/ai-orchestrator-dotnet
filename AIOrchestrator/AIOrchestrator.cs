namespace AIOrchestrator;

using AIOrchestrator.Support;

public class AIManager
{
    private readonly OllamaClient _ollamaClient = new();

    public async Task StartAsync()
    {
        Console.WriteLine("Enter your input:");
        var input = Console.ReadLine()!;

        var instructionJsonResponse = await JsonOrchestratorAsync(input);
        var instructionJson = MarkdownProcess.RemoveCodeMarkdown(instructionJsonResponse);
        Console.WriteLine($"Processed JSON:\n{instructionJson}");

        var output = MethodInvoker.Execute(instructionJson, this);
        Console.WriteLine($"Output: {output}");
    }

    private async Task<string> RequestAIAsync(string prompt)
    {
        var response = await _ollamaClient.RequestAsync(prompt: prompt, model: "gemma3");
        return response.Response;
    }

    private async Task<string> JsonOrchestratorAsync(string input)
    {
        var prompt = $@"
Analyze the input '{input}' and determine which function to call.

You have functions and parameters {{FunctionName(parameterName)}}:
1. WeatherForecast(location) - returns a weather forecast for the given location.
2. GetLocation() - returns a location name.

Response should be in JSON format:
{{
   ""Function"": ""FunctionName"",
   ""Parameters"": [""Parameter1"", ""Parameter2""]
}}

Response only with JSON format, no other text.
Do not include any explanations or additional text.
Return only JSON body.
";

        return await RequestAIAsync(prompt);
    }

#pragma warning disable IDE0051 // Remove unused private members
    private static string WeatherForecast(string location) => $"Its sunny and warm in {location}";

    private static string GetLocation() => "Dubai";
}