using System.Diagnostics.Metrics;

namespace AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast
{
    public class WeatherForecastTelemetry : System.Diagnostics.Telemetry<WeatherForecastTelemetry>
    {
        public WeatherForecastTelemetry(ILoggerFactory loggerFactory, IMeterFactory meterFactory, System.Diagnostics.TelemetryOptions<WeatherForecastTelemetry>? options = null) 
            : base(loggerFactory, meterFactory, options)
        {
            TemperatureC = Meter.CreateHistogram<int>("weather_forecast.temperature", unit: "ºC");
        }

        public Histogram<int> TemperatureC { get; }
    }
}
