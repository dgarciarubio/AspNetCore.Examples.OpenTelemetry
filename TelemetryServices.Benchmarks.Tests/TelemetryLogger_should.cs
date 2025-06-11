using BenchmarkDotNet.Reports;
using TelemetryServices.Benchmarks.Tests.Collections;
using TelemetryServices.Benchmarks.Tests.Fixtures;

namespace TelemetryServices.Benchmarks.Tests;

[Collection<RunSequentially>]
public class TelemetryLogger_should(BenchmarkFixture<LoggerBenchmarks> benchmarkFixture) 
    : IClassFixture<BenchmarkFixture<LoggerBenchmarks>>
{
    [Fact]
    public void Perform_comparably_to_standard_logger_when_not_enriched()
    {
        var execTimes = new
        {
            Standard = benchmarkFixture.GetAverageExecutionTimeNs(b => b.LogStandard()),
            TelemetryOfTName = benchmarkFixture.GetAverageExecutionTimeNs(b => b.LogTelemetryOfTName()),
            Telemetry = benchmarkFixture.GetAverageExecutionTimeNs(b => b.LogTelemetry()),
        };

        Assert.InRange(execTimes.TelemetryOfTName / execTimes.Standard, 0, 1.01);
        Assert.InRange(execTimes.Telemetry / execTimes.Standard, 0, 1.01);
    }

    [Fact]
    public void Perform_comparably_to_standard_logger_when_not_enriched_high_perf()
    {
        var execTimes = new
        {
            Standard = benchmarkFixture.GetAverageExecutionTimeNs(b => b.LogStandardHighPerf()),
            TelemetryOfTName = benchmarkFixture.GetAverageExecutionTimeNs(b => b.LogTelemetryOfTNameHighPerf()),
            Telemetry = benchmarkFixture.GetAverageExecutionTimeNs(b => b.LogTelemetryHighPerf()),
        };

        Assert.InRange(execTimes.TelemetryOfTName / execTimes.Standard, 0, 1.01);
        Assert.InRange(execTimes.Telemetry / execTimes.Standard, 0, 1.01);
    }

    [Fact]
    public void Perform_comparably_to_standard_logger_with_scopes_when_enriched()
    {
        var execTimes = new
        {
            Standard = benchmarkFixture.GetAverageExecutionTimeNs(b => b.LogStandardWithScope()),
            TelemetryOfTName = benchmarkFixture.GetAverageExecutionTimeNs(b => b.LogTelemetryOfTNameEnriched()),
            Telemetry = benchmarkFixture.GetAverageExecutionTimeNs(b => b.LogTelemetryEnriched()),
        };

        Assert.InRange(execTimes.TelemetryOfTName / execTimes.Standard, 0, 1.01);
        Assert.InRange(execTimes.Telemetry / execTimes.Standard, 0, 1.01);
    }

    [Fact]
    public void Perform_comparably_to_standard_logger_with_scopes_when_enriched_high_perf()
    {
        var execTimes = new
        {
            Standard = benchmarkFixture.GetAverageExecutionTimeNs(b => b.LogStandardWithScopeHighPerf()),
            TelemetryOfTName = benchmarkFixture.GetAverageExecutionTimeNs(b => b.LogTelemetryOfTNameEnrichedHighPerf()),
            Telemetry = benchmarkFixture.GetAverageExecutionTimeNs(b => b.LogTelemetryEnrichedHighPerf()),
        };

        Assert.InRange(execTimes.TelemetryOfTName / execTimes.Standard, 0, 1.01);
        Assert.InRange(execTimes.Telemetry / execTimes.Standard, 0, 1.01);
    }
}
