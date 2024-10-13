using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AspNetCore.Examples.OpenTelemetry.Api.SampleTelemetry;

public static class SampleTelemetryEndpoints
{
    public static IServiceCollection AddSampleTelemetry(this IServiceCollection services)
    {
        services
            .AddSingleton<Telemetry>()
            .AddOpenTelemetry()
            .WithTracing(t => t.AddSource(Telemetry.ActivitySourceName))
            .WithMetrics(m => m.AddMeter(Telemetry.MeterName));
        return services;
    }

    public static IEndpointRouteBuilder MapSampleTelemetryEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("telemetry/traces", async (Telemetry telemetry, int durationMs = 0, string? tag = null) =>
        {
            var tags = new Dictionary<string, object?>() { { "tag", tag } };
            using var trace = telemetry.ActivitySource.StartActivity(name: "sample_activity", kind: ActivityKind.Internal, tags: tags);

            await Task.Delay(durationMs);

            return Results.Ok();
        });

        builder.MapGet("telemetry/metrics", (Telemetry telemetry, int value = 0, string? tag = null) =>
        {
            var tags = new Dictionary<string, object?>() { { "tag", tag } }.ToArray();

            telemetry.Histogram.Record(value, tags);
            telemetry.Counter.Add(value, tags);
            telemetry.Gauge.Record(value, tags);
            telemetry.UpDownCounter.Add(value, tags);

            return Results.Ok();
        });


        builder.MapGet("telemetry/logs", (Telemetry telemetry, LogLevel level = LogLevel.Information, string? attributeValue = null) =>
        {
            telemetry.Logger.Log(level, "Sample log message with attribute value: \"{attribute}\"", attributeValue);

            return Results.Ok();
        });

        return builder;
    }

    private sealed class Telemetry : IDisposable
    {
        public const string LoggerName = "sample_logger";
        public const string MeterName = "sample_meter";
        public const string ActivitySourceName = "sample_source";
        public readonly ILogger Logger;
        public readonly ActivitySource ActivitySource;
        public readonly Meter Meter;
        public readonly Histogram<int> Histogram;
        public readonly Counter<int> Counter;
        public readonly Gauge<int> Gauge;
        public readonly UpDownCounter<int> UpDownCounter;

        public Telemetry(ILoggerFactory loggerFactory, IMeterFactory meterFactory)
        {
            Logger = loggerFactory.CreateLogger(LoggerName);
            ActivitySource = new ActivitySource(ActivitySourceName, version: "1.0");
            Meter = meterFactory.Create(MeterName);
            Histogram = Meter.CreateHistogram<int>(name: "sample_histogram", unit: "Units", description: "Sample Histogram description");
            Counter = Meter.CreateCounter<int>(name: "sample_counter", unit: "Units", description: "Sample Counter description");
            Gauge = Meter.CreateGauge<int>(name: "sample_gauge", unit: "Units", description: "Sample Gauge description");
            UpDownCounter = Meter.CreateUpDownCounter<int>(name: "sample_up_down_counter", unit: "Units", description: "Sample Up Down Counter description");
        }

        public void Dispose()
        {
            ActivitySource.Dispose();
            Meter.Dispose();
        }
    }
}