using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelemetryServices.Benchmarks.BenchmarkDoubles;

namespace TelemetryServices.Benchmarks;

[MemoryDiagnoser]
public class LoggerBenchmarks : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    private readonly ILogger<StandardLoggerName> _standardLogger;
    private readonly ILogger<TelemetryName> _telemetryOfTName;
    private readonly ILogger<TelemetryNameEnriched> _telemetryOfTNameEnriched;
    private readonly ILogger _namedTelemetry;
    private readonly ILogger _namedTelemetryEnriched;

    public LoggerBenchmarks()
    {
        _serviceProvider = new ServiceCollection()
            .AddLogging(l => l.AddProvider(LoggingListener.Instance))
            .AddTelemetry(telemetry =>
            {
                telemetry.AddFor("Named", o =>
                {
                    o.Version = "1.0";
                    o.Tags = new Dictionary<string, object?>()
                    {
                        ["Tag1"] = "Value1",
                        ["Tag2"] = "Value2",
                    };
                    o.Logger.Version = null;
                    o.Logger.Tags = null;
                });
                telemetry.AddFor<TelemetryName>(o =>
                {
                    o.Version = "1.0";
                    o.Tags = new Dictionary<string, object?>()
                    {
                        ["Tag1"] = "Value1",
                        ["Tag2"] = "Value2",
                    };
                    o.Logger.Version = null;
                    o.Logger.Tags = null;
                });
                telemetry.AddFor("NamedEnriched", o =>
                {
                    o.Version = "1.0";
                    o.Tags = new Dictionary<string, object?>()
                    {
                        ["Tag1"] = "Value1",
                        ["Tag2"] = "Value2",
                    };
                });
                telemetry.AddFor<TelemetryNameEnriched>(o =>
                {
                    o.Version = "1.0";
                    o.Tags = new Dictionary<string, object?>()
                    {
                        ["Tag1"] = "Value1",
                        ["Tag2"] = "Value2",
                    };
                });
            })
            .BuildServiceProvider();

        _standardLogger = _serviceProvider.GetRequiredService<ILogger<StandardLoggerName>>();
        _telemetryOfTName = _serviceProvider.GetRequiredService<ITelemetry<TelemetryName>>().Logger;
        _namedTelemetry = _serviceProvider.GetRequiredKeyedService<ITelemetry>("Named").Logger;
        _telemetryOfTNameEnriched = _serviceProvider.GetRequiredService<ITelemetry<TelemetryNameEnriched>>().Logger;
        _namedTelemetryEnriched = _serviceProvider.GetRequiredKeyedService<ITelemetry>("NamedEnriched").Logger;
    }

    [Benchmark]
    public void LogStandard()
    {
        _standardLogger.LogInformation("Log from {LoggerKind}", nameof(StandardLoggerName));
    }

    [Benchmark]
    public void LogStandardHighPerf()
    {
        _standardLogger.LogFrom(nameof(StandardLoggerName));
    }

    [Benchmark]
    public void LogStandardWithScope()
    {
        using var scope = _standardLogger.BeginScope(new Dictionary<string, object?>
        {
            ["Version"] = "1.0",
            ["Tag1"] = "Value1",
            ["Tag2"] = "Value2",
        });
        _standardLogger.LogInformation("Log from {LoggerKind}", nameof(StandardLoggerName));
    }

    [Benchmark]
    public void LogStandardWithScopeHighPerf()
    {
        using var scope = _standardLogger.BeginScope(new Dictionary<string, object?>
        {
            ["Version"] = "1.0",
            ["Tag1"] = "Value1",
            ["Tag2"] = "Value2",
        });
        _standardLogger.LogFrom(nameof(StandardLoggerName));
    }

    [Benchmark]
    public void LogTelemetryOfTName()
    {
        _telemetryOfTName.LogInformation("Log from {LoggerKind}", nameof(TelemetryName));
    }

    [Benchmark]
    public void LogTelemetryOfTNameHighPerf()
    {
        _telemetryOfTName.LogFrom(nameof(TelemetryName));
    }

    [Benchmark]
    public void LogTelemetryOfTNameEnriched()
    {
        _telemetryOfTNameEnriched.LogInformation("Log from {LoggerKind}", nameof(TelemetryNameEnriched));
    }

    [Benchmark]
    public void LogTelemetryOfTNameEnrichedHighPerf()
    {
        _telemetryOfTNameEnriched.LogFrom(nameof(TelemetryNameEnriched));
    }

    [Benchmark]
    public void LogTelemetry()
    {
        _namedTelemetry.LogInformation("Log from {LoggerKind}", "Name");
    }

    [Benchmark]
    public void LogTelemetryHighPerf()
    {
        _namedTelemetry.LogFrom("Name");
    }

    [Benchmark]
    public void LogTelemetryEnriched()
    {
        _namedTelemetryEnriched.LogInformation("Log from {LoggerKind}", "NamedEnriched");
    }

    [Benchmark]
    public void LogTelemetryEnrichedHighPerf()
    {
        _namedTelemetryEnriched.LogFrom("NamedEnriched");
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }

    private class StandardLoggerName { }

    private class TelemetryNameEnriched { }

    private class TelemetryName { }
}

public static partial class LoggerExtensions
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Log from {LoggerKind}"
    )]
    public static partial void LogFrom(this ILogger logger, string loggerKind);
}
