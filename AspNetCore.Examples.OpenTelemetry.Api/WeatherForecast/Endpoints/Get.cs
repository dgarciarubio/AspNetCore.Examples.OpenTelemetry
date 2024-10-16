using AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast.Services;
using System.Diagnostics;

namespace AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast.Endpoints;

internal static class Get
{
    private static readonly IReadOnlyList<string> _summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public static IEndpointRouteBuilder MapGet(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/weather-forecast", (ITelemetry telemetry) =>
        {
            using var activity = telemetry.ActivitySource.StartActivity(name: "weather_forecast.request", kind: ActivityKind.Internal);

            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    _summaries[Random.Shared.Next(_summaries.Count)]
                ))
                .ToArray();

            telemetry.Logger.LogInformation("Weather forecast calculated: {@forecast}", forecast);
            foreach (var item in forecast)
            {
                telemetry.TemperatureC.Record(item.TemperatureC);
            }

            return forecast;
        })
        .WithName("GetWeatherForecast");

        return endpoints;
    }
}
