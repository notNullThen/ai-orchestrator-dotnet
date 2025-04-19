namespace AIOrchestrator.Weather;

public class WeatherForecast()
{
#pragma warning disable IDE0051 // Remove unused private members
    private static string GetWeather(string location) => $"It's sunny and warm in {location}";

    private static string GetLocation() => "Dubai";

    private static void Exit() => AIManager.Exit();
}
