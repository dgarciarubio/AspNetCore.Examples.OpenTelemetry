namespace AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast;

public record WeatherForecast(DateOnly Date, int TemperatureC)
{
    public int TemperatureF { get; } = 32 + (int)(TemperatureC / 0.5556);

    public string Summary { get; } = TemperatureC switch
    {
        (< -10) => "Freezing",
        (< 0) => "Bracing",
        (< 10) => "Cool",
        (< 20) => "Mild",
        (< 25) => "Warm",
        (< 30) => "Balmy",
        (< 35) => "Hot",
        (< 40) => "Sweltering",
        (>= 40) => "Scorching",
    };
}