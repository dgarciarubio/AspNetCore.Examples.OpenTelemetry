using AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast.Endpoints;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast;

internal static class Extensions
{
    public static IServiceCollection AddWeatherForecast(this IServiceCollection services)
    {
        return services
            .AddSingleton<WeatherForecastTelemetry>()
            .ConfigureOpenTelemetryTracerProvider(t => t.AddSource(WeatherForecastTelemetry.Name))
            .ConfigureOpenTelemetryMeterProvider(t => t.AddMeter(WeatherForecastTelemetry.Name));
    }

    public static IEndpointRouteBuilder MapWeatherForecastEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet();
        return endpoints;
    }
}
