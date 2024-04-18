using System.Diagnostics.Metrics;
using System.Diagnostics;

public static class TelemetryEndpoints
{
    private static readonly Delegate Traces = (int durationMs = 0, string? tag = null) =>
    {
        using var source = new ActivitySource(name: "sample_source", version: "1.0");
        using var trace = source.StartActivity(name: "sample_activity", kind: ActivityKind.Internal, tags: new Dictionary<string, object?>()
        {
            { "tag", tag }
        });

        return Results.Ok();
    };

    private static readonly Delegate Metrics = (IMeterFactory meterFactory, int value = 0, string? tag = null) =>
    {
        var tags = new Dictionary<string, object?>()
        {
            { "tag", tag }
        }.ToArray();

        var meter = meterFactory.Create("sample_meter");
        var counter = meter.CreateCounter<int>(name: "sample_counter", unit: "Units", description: "Sample Counter description");
        var histogram = meter.CreateHistogram<int>(name: "sample_histogram", unit: "Units", description: "Sample Histogram description");

        counter.Add(value, tags);
        histogram.Record(value, tags);

        return Results.Ok();
    };

    private static readonly Delegate Logs = (ILoggerFactory loggerFactory, LogLevel level = LogLevel.Information, string? attributeValue = null) =>
    {
        var logger = loggerFactory.CreateLogger(typeof(TelemetryEndpoints));

        logger.Log(level, "Sample log message with attribute value: \"{attribute}\"", attributeValue);

        return Results.Ok();
    };

    public static IEndpointRouteBuilder MapTelemetryEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("telemetry/traces", Traces);
        builder.MapGet("telemetry/metrics", Metrics);
        builder.MapGet("telemetry/logs", Logs);
        return builder;
    }
}