using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast;

internal static class TelemetryExtensions
{
    extension (ITelemetry<WeatherForecast> telemetry)
    {
        public Histogram<int> TemperatureC => telemetry.Meter.CreateHistogram<int>("weather_forecast.temperature", unit: "ºC");
    }
}
