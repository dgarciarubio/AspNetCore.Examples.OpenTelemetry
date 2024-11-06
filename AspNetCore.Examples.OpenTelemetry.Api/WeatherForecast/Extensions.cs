using AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast.Endpoints;
using AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast.Services;

namespace AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast;

internal static class Extensions
{
    public static IServiceCollection AddWeatherForecast(this IServiceCollection services)
    {
        services.AddTelemetry();
        return services;
    }

    public static IEndpointRouteBuilder MapWeatherForecastEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet();
        return endpoints;
    }
}
