#pragma warning disable CA1822 // Mark members as static

namespace AIOrchestrator.Application;

using Core.AiAppFacade;
using Core.AiAppFacade.Types;

public sealed class AppSample : AiAppFacadeBase
{
    public string GetWeather(string location) => $"It's sunny and warm in {location}";

    public string GetLocation() => "Dubai";

    public override string GetConstraints() =>
        @$"
1. If History already contains the answer, call {nameof(Exit)}().
2. Never repeat a function call found in History.
3. Correct grammar/casing in parameters.
4. Return ONLY raw JSON. No markdown, no backticks.
";

    public override AppDescription GetDescription() =>
        [
            new()
            {
                Name = nameof(GetWeather),
                Description = "Returns a weather forecast for the given location.",
                Parameters =
                [
                    new()
                    {
                        Name = "location",
                        Description = "The location for which to retrieve the weather forecast.",
                    },
                ],
            },
            new()
            {
                Name = nameof(GetLocation),
                Description = "Retrieves the current location.",
                Parameters = [],
            },
            new()
            {
                Name = nameof(Exit),
                Description = "Terminates the program.",
                Parameters = [],
            },
        ];
}
