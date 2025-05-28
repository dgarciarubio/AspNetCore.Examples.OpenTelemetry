using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast;

internal static class TelemetryExtensions
{
    public static Histogram<int> TemperatureC(this ITelemetry<WeatherForecast> telemetry)
    {
        return telemetry.Meter.CreateHistogram<int>("weather_forecast.temperature", unit: "ºC");
    }
}
