using AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast.Endpoints;
using System.Diagnostics;

namespace AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast;

internal static class Extensions
{
    public static IServiceCollection AddWeatherForecast(this IServiceCollection services)
    {
        return services
            .AddTelemetry(new TelemetryOptions<WeatherForecast>()
            {
                Tags = [
                    new ("Tag1", "Value1"),
                    new ("Tag2", "Value2"),
                ],
                Version = "3.5",
            });
    }

    public static IEndpointRouteBuilder MapWeatherForecastEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet();
        return endpoints;
    }
}
