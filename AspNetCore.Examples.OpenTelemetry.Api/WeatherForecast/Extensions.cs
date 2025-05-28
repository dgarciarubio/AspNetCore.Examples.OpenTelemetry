using AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast.Endpoints;
using System.Diagnostics;

namespace AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast;

internal static class Extensions
{
    public static IServiceCollection AddWeatherForecast(this IServiceCollection services)
    {
        return services.AddToOpenTelemetryProviders<WeatherForecast>();
    }

    public static IEndpointRouteBuilder MapWeatherForecastEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet();
        return endpoints;
    }
}
