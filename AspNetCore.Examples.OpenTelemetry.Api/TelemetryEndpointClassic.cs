using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AspNetCore.Examples.OpenTelemetry.Api
{
    public static partial class TelemetryEndpointClassic
    {
        public static IServiceCollection AddClassicTelemetryEndpoint(this IServiceCollection services)
        {
            services
                .ConfigureOpenTelemetryTracerProvider(t => t.AddSource(Telemetry.Name))
                .ConfigureOpenTelemetryMeterProvider(t => t.AddMeter(Telemetry.Name));

            return services;
        }

        public static WebApplication MapClassicTelemetryEndpoint(this WebApplication app)
        {
            app.MapGet("/telemetry/classic", async (ILogger<ClassicTelemetryName> logger, int delay = 0) =>
            {
                using var versionScope = logger.BeginScope(new Dictionary<string, object?> { [nameof(Telemetry.Version)] = Telemetry.Version });
                using var tagsScope = logger.BeginScope(Telemetry.Tags);

                using var activity = Telemetry.ActivitySource.StartActivity(name: "telemetry.classic");
                await Task.Delay(delay);

                logger.LogTelemetryEndpointCall(delay);

                Telemetry.Calls.Add(1);
                Telemetry.Delay.Record(delay);
            });

            return app;
        }

        private static partial class Telemetry
        {
            public static readonly string Name = $"{typeof(ClassicTelemetryName).Namespace}.{nameof(ClassicTelemetryName)}";
            public static readonly string Version = "1.0";
            public static readonly Dictionary<string, object?> Tags = new Dictionary<string, object?>
            {
                ["Tag1"] = "Value1",
                ["Tag2"] = "Value2",
            };

            public static readonly ActivitySource ActivitySource = new ActivitySource(Name, Version, Tags);
            public static readonly Meter Meter = new Meter(Name, Version, Tags);
            public static readonly Counter<int> Calls = Meter.CreateCounter<int>("telemetry.classic.calls");
            public static readonly Histogram<int> Delay = Meter.CreateHistogram<int>("telemetry.classic.delay");
        }

        [LoggerMessage(LogLevel.Information, "Telemetry endpoint called with delay = {delay}")]
        private static partial void LogTelemetryEndpointCall(this ILogger<ClassicTelemetryName> logger, int delay);
    }

    public class ClassicTelemetryName { }
}
