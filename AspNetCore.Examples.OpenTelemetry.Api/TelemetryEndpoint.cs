using AspNetCore.Examples.OpenTelemetry.TelemetryServices;
using System.Diagnostics;
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
            app.MapGet("/telemetry", async (SampleTelemetryService telemetry) =>
            {
                using var _ = telemetry.ActivitySource.StartActivity(name: "telemetry.sample");

                var timeStamp = Stopwatch.GetTimestamp();
                telemetry.Logger.LogInformation("Telemetry endpoint called");

                await Task.Delay(Random.Shared.Next(100));
                
                telemetry.Calls.Add(1);
                telemetry.Delay.Record(Stopwatch.GetElapsedTime(timeStamp).TotalMilliseconds);
            });

            return app;
        }

        internal class SampleTelemetryService : Telemetry<SampleTelemetryName>
        {
            public SampleTelemetryService(ILoggerFactory loggerFactory, IMeterFactory meterFactory)
                : base(loggerFactory, meterFactory)
            {
                Calls = Meter.CreateCounter<int>("telemetry.sample.calls", description: "Number of times the endpoint has been called");
                Delay = Meter.CreateHistogram<double>("telemetry.sample.delay", unit: "ms", description: "Delay in milliseconds of service calls");
            }

            public Counter<int> Calls { get; }
            public Histogram<double> Delay { get; }
        }
    }

    public class SampleTelemetryName { }
}
