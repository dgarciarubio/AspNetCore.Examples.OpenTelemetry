using AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast.Services;
//using System.Diagnostics;

namespace AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast.Endpoints;

internal static class Get
{
    public static IEndpointRouteBuilder MapGet(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/weather-forecast", (ITelemetry telemetry) =>
        {
            using var activity = telemetry.ActivitySource.StartActivity(name: "weather_forecast.request", kind: System.Diagnostics.ActivityKind.Internal);

            return Enumerable.Range(1, 5).Select(index =>
            {
                var item = new WeatherForecast
                (
                    Date: DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC: Random.Shared.Next(-20, 55)
                );
                telemetry.Logger.LogInformation("Weather forecast calculated for {Date}: {Summary}, {TemperatureC}ºC ({TemperatureF}ºF)",
                    item.Date.ToString("yyyy-MM-dd"),
                    item.Summary,
                    item.TemperatureC,
                    item.TemperatureF
                );
                telemetry.TemperatureC.Record(item.TemperatureC, new System.Diagnostics.TagList
                {
                    { "Date", item.Date.ToString("yyyy-MM-dd") },
                    { "Summary", item.Summary }
                });
                return item;
            });
        })
        .WithName("GetWeatherForecast");

        return endpoints;
    }
}
