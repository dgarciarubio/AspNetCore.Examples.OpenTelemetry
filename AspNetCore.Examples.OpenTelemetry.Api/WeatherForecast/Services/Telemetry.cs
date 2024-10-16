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
        TemperatureC = Meter.CreateHistogram<int>("temperature", unit: "ºC");
    }

    public Histogram<int> TemperatureC { get; }
}

internal static class TelemetryExtensions
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services)
    {
        services.AddSingleton<ITelemetry, Telemetry>();
        services.AddOpenTelemetry()
            .WithTracing(t => t.AddSource(Telemetry.CategoryName))
            .WithMetrics(t => t.AddMeter(Telemetry.CategoryName));
        return services;
    }
}
