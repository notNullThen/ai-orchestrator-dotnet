namespace AIOrchestrator;

using AIOrchestrator.Support;

public class AIManager
{
    private readonly OllamaClient _ollamaClient = new();

    public async Task<string> RequestAIAsync(string prompt)
    {
        var response = await _ollamaClient.RequestAsync(prompt, Roles.User, "mistral");
        return response.Response;
    }

    public async Task StartAsync()
    {
        var calculatorResponse = await RequestAIAsync("You are calculator. Respond only with the result of the calculation.\n\n 5+8");
        Console.WriteLine(calculatorResponse);

        var converterResponse = await RequestAIAsync($"You need to convert kilograms to grams. Respond only with the result of the conversion.\n\n {calculatorResponse} kgs");
        Console.WriteLine(converterResponse);
    }
}