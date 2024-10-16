using System.Diagnostics;

namespace AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast;

internal static class WeatherForecastExtensions
{
    public static IServiceCollection AddWeatherForecast(this IServiceCollection services)
    {
        services.AddTelemetry<IWeatherForecastTelemetry, WeatherForecastTelemetry>();
        return services;
    }

    public static IEndpointRouteBuilder MapWeatherForecast(this IEndpointRouteBuilder endpoints)
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        endpoints.MapGet("/weatherforecast", (IWeatherForecastTelemetry telemetry) =>
        {
            using var _ = telemetry.ActivitySource.StartActivity(name: "sample_activity", kind: ActivityKind.Internal);
            telemetry.Logger.LogInformation("Requesting weather forecast");

            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
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
