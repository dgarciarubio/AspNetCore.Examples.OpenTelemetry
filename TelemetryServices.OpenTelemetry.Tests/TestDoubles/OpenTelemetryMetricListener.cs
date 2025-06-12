using OpenTelemetry;
using OpenTelemetry.Metrics;
using System.Collections.Concurrent;

namespace TelemetryServices.OpenTelemetry.Tests.TestDoubles;

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
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_exportIntervalMilliseconds));
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMilliseconds));
        do
        {
            if (!_exporter.Metrics.IsEmpty)
            {
                return _exporter.Metrics;
            }
            await timer.WaitForNextTickAsync();
        }
        while (!cts.Token.IsCancellationRequested);
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
