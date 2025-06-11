using OpenTelemetry;
using OpenTelemetry.Metrics;
using System.Collections.Concurrent;

namespace AspNetCore.Examples.OpenTelemetry.OpenTelemetryServices.Tests.TestDoubles;

internal class OpenTelemetryMetricListener : PeriodicExportingMetricReader
{
    private const int _exportIntervalMilliseconds = 5;

    private readonly CustomMetricExporter _exporter;

    public OpenTelemetryMetricListener()
        : base(new CustomMetricExporter(), _exportIntervalMilliseconds)
    {
        _exporter = (CustomMetricExporter)exporter;
    }

    public async Task<IEnumerable<Metric>> WaitForMetrics(int timeoutMilliseconds = _exportIntervalMilliseconds * 10)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMilliseconds));
        return await WaitForMetrics(cts.Token);
    }

    public async Task<IEnumerable<Metric>> WaitForMetrics(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_exportIntervalMilliseconds));
        while (_exporter.Metrics.Count == 0)
        {
            await timer.WaitForNextTickAsync(cancellationToken);
        }
        return _exporter.Metrics;
    }

    private class CustomMetricExporter : BaseExporter<Metric>
    {
        public ConcurrentQueue<Metric> Metrics { get; } = [];

        public override ExportResult Export(in Batch<Metric> batch)
        {
            foreach (var metric in batch)
            {
                Metrics.Enqueue(metric);
            }

            return ExportResult.Success;
        }
    }
}
