using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast;

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
