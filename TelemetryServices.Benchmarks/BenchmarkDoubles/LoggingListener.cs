using Microsoft.Extensions.Logging;

namespace TelemetryServices.Benchmarks.BenchmarkDoubles;

internal class LoggingListener : ILoggerProvider
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