using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast.Services;

internal interface IWeatherForecastTelemetry : ITelemetry<WeatherForecast>
{
    Histogram<int> TemperatureC { get; }
}

internal class WeatherForecastTelemetry : Telemetry<WeatherForecast>, IWeatherForecastTelemetry
{
    public WeatherForecastTelemetry(ILoggerFactory loggerFactory, IMeterFactory meterFactory)
        : base(loggerFactory, meterFactory)
    {
        TemperatureC = Meter.CreateHistogram<int>("temperature", unit: "ºC");
    }

    public Histogram<int> TemperatureC { get; }
}

internal static class TelemetryExtensions
{
    public static IServiceCollection AddWeatherForecastTelemetry(this IServiceCollection services)
    {
        return services
            .AddSingleton<IWeatherForecastTelemetry, WeatherForecastTelemetry>()
            .ConfigureOpenTelemetryTracerProvider((serviceProvider, traces) =>
            {
                var telemetry = serviceProvider.GetRequiredService<IWeatherForecastTelemetry>();
                traces.AddSource(telemetry.ActivitySource.Name);
            })
            .ConfigureOpenTelemetryMeterProvider((serviceProvider, meters) =>
            {
                var telemetry = serviceProvider.GetRequiredService<IWeatherForecastTelemetry>();
                meters.AddMeter(telemetry.Meter.Name);
            });
    }
}
