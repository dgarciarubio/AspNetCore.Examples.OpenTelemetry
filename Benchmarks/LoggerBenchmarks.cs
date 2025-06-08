using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices.Benchmarks;

[MemoryDiagnoser]
public class LoggerBenchmarks : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    private readonly ILogger<StandardLogger> _standardLogger;
    private readonly ILogger<GenericTelemetryLogger> _genericTelemetryLogger;
    private readonly ILogger _namedTelemetryLogger;

    public LoggerBenchmarks()
    {
        _serviceProvider = new ServiceCollection()
            .AddTelemetry(telemetry =>
            {
                telemetry.For(nameof(NamedTelemetryLogger)).Configure(o => {
                    o.Version = "1.0";
                    o.Tags = new() 
                    { 
                        ["Tag1"] = "Value1", 
                        ["Tag2"] = "Value2", 
                    };
                });
                telemetry.For<GenericTelemetryLogger>().Configure(o => {
                    o.Version = "1.0";
                    o.Tags = new()
                    {
                        ["Tag1"] = "Value1",
                        ["Tag2"] = "Value2",
                    };
                });
            })
            .AddLogging(l => l.AddProvider(LoggingListener.Instance))
            .BuildServiceProvider();

        _standardLogger = _serviceProvider.GetRequiredService<ILogger<StandardLogger>>();
        _genericTelemetryLogger = _serviceProvider.GetRequiredService<ITelemetry<GenericTelemetryLogger>>().Logger;
        _namedTelemetryLogger = _serviceProvider.GetRequiredKeyedService<ITelemetry>(nameof(NamedTelemetryLogger)).Logger;
    }

    [Benchmark]
    public void LogStandardCompileTime()
    {
        _standardLogger.LogFrom(nameof(StandardLogger));
    }

    [Benchmark]
    public void LogStandard()
    {
        _standardLogger.LogInformation("Log from {LoggerKind}", nameof(StandardLogger));
    }

    [Benchmark]
    public void LogGenericCompileTime()
    {
        _genericTelemetryLogger.LogFrom(nameof(GenericTelemetryLogger));
    }

    [Benchmark]
    public void LogGeneric()
    {
        _genericTelemetryLogger.LogInformation("Log from {LoggerKind}", nameof(GenericTelemetryLogger));
    }

    [Benchmark]
    public void LogNamedCompileTime()
    {
        _namedTelemetryLogger.LogFrom(nameof(NamedTelemetryLogger));
    }

    [Benchmark]
    public void LogNamed()
    {
        _namedTelemetryLogger.LogInformation("Log from {LoggerKind}", nameof(NamedTelemetryLogger));
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    private class StandardLogger { }

    private class GenericTelemetryLogger { }

    private class NamedTelemetryLogger { }

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
