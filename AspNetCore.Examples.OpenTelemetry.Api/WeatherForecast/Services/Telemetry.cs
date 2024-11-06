using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast.Services;

internal interface ITelemetry : ITelemetry<WeatherForecast>
{
    Histogram<int> TemperatureC { get; }
}

internal class Telemetry : Telemetry<WeatherForecast>, ITelemetry
{
    public Telemetry(ILoggerFactory loggerFactory, IMeterFactory meterFactory)
        : base(loggerFactory, meterFactory)
    {
        TemperatureC = Meter.CreateHistogram<int>("weather_forecast.temperature", unit: "ºC");
    }

    public Histogram<int> TemperatureC { get; }
}

internal static class WeatherForecastTelemetryExtensions
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services)
    {
        return services
            .AddSingleton<ITelemetry, Telemetry>()
            .ConfigureOpenTelemetryTracerProvider(t => t.AddSource(Telemetry.Name))
            .ConfigureOpenTelemetryMeterProvider(m => m.AddMeter(Telemetry.Name));
    }
}
