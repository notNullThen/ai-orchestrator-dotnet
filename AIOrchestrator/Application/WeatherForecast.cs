namespace AIOrchestrator.Application;

using AIOrchestrator.Support.Types;

public class WeatherForecast : AiFacadeBase
{
    private static string GetWeather(string location) => $"It's sunny and warm in {location}";

    private static string GetLocation() => "Dubai";

    public override string GetDescription() =>
        @$"
1. {nameof(GetWeather)}(location):
    - Description: Returns a weather forecast for the given location.
    - Parameter:
        - location (string): The location for which to retrieve the weather forecast.

2. {nameof(GetLocation)}():
    - Description: Retrieves the current location.
    - Parameters: None.

3. {nameof(Exit)}():
    - Description: Terminates the program.
    - Parameters: None.
    ";
}
