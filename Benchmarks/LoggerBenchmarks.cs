using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices.Benchmarks;

[MemoryDiagnoser]
public class LoggerBenchmarks : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    private readonly ILogger<StandardLoggerName> _standardLogger;
    private readonly ILogger<TelemetryName> _telemetryOfTName;
    private readonly ILogger _namedTelemetry;

    public LoggerBenchmarks()
    {
        _serviceProvider = new ServiceCollection()
            .AddLogging(l => l.AddProvider(LoggingListener.Instance))
            .AddTelemetry(telemetry =>
            {
                telemetry.AddFor("Name", o =>
                {
                    o.Version = "1.0";
                    o.Tags = new Dictionary<string, object?>()
                    {
                        ["Tag1"] = "Value1",
                        ["Tag2"] = "Value2",
                    };
                });
                telemetry.AddFor<TelemetryName>(o =>
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
        _namedTelemetry = _serviceProvider.GetRequiredKeyedService<ITelemetry>("Name").Logger;
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
    public void LogStandardHighPerfWithScope()
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
    public void LogNamed()
    {
        _namedTelemetry.LogInformation("Log from {LoggerKind}", "named");
    }

    [Benchmark]
    public void LogNamedHighPerf()
    {
        _namedTelemetry.LogFrom("named");
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }

    private class StandardLoggerName { }

    private class TelemetryName { }

    private class LoggingListener : ILoggerProvider
    {
        public static readonly LoggingListener Instance = new();

        public ILogger CreateLogger(string categoryName) => CatchAllLogger.Instance;

        public void Dispose() { }

        private class CatchAllLogger : ILogger
        {
            public static readonly CatchAllLogger Instance = new();

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
        }
    }
}

public static partial class LoggerExtensions
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Log from {LoggerKind}"
    )]
    public static partial void LogFrom(this ILogger logger, string loggerKind);
}
