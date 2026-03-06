#pragma warning disable CA1822 // Mark members as static

namespace AIOrchestrator.Application;

using AIOrchestrator.Core.AiAppFacade;
using AIOrchestrator.Core.AiAppFacade.Types;

public sealed class AppSample : AiAppFacadeBase
{
    public string GetWeather(string location) => $"It's sunny and warm in {location}";

    public string GetLocation() => "Dubai";

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
