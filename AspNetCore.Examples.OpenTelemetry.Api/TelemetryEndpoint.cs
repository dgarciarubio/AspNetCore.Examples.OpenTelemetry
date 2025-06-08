using AspNetCore.Examples.OpenTelemetry.TelemetryServices;
using System.Diagnostics.Metrics;

namespace AspNetCore.Examples.OpenTelemetry.Api
{
    public static partial class TelemetryEndpoint
    {
        public static IServiceCollection AddTelemetryEndpoint(this IServiceCollection services)
        {
            services.AddTelemetry<SampleTelemetryService>()
                .AddToOpenTelemetryProviders();

            return services;
        }

        public static WebApplication MapTelemetryEndpoint(this WebApplication app)
        {
            app.MapGet("/telemetry", async (SampleTelemetryService telemetry, int delay = 0) =>
            {
                using var _ = telemetry.ActivitySource.StartActivity(name: "telemetry.sample");
                await Task.Delay(delay);

                telemetry.Logger.LogInformation("Telemetry endpoint called with delay = {delay}", delay);

                telemetry.Calls.Add(1);
                telemetry.Delay.Record(delay);
            });

            return app;
        }

        internal class SampleTelemetryService : Telemetry<SampleTelemetryName>
        {
            public SampleTelemetryService(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions<SampleTelemetryService> options)
                : base(loggerFactory, meterFactory, options)
            {
                Calls = Meter.CreateCounter<int>("telemetry.sample.calls");
                Delay = Meter.CreateHistogram<int>("telemetry.sample.delay");
            }

            public Counter<int> Calls { get; }
            public Histogram<int> Delay { get; }
        }
    }

    public class SampleTelemetryName { }
}
