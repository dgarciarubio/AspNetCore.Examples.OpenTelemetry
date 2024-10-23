using AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast.Endpoints;
using AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast.Services;

namespace AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast;

internal static class WeatherForecastExtensions
{
    public static IServiceCollection AddWeatherForecast(this IServiceCollection services)
    {
        services.AddWeatherForecastTelemetry();
        return services;
    }

    public static IEndpointRouteBuilder MapWeatherForecast(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGetWeatherForecast();
        return endpoints;
    }
}
