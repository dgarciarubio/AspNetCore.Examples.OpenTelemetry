using Microsoft.Extensions.Logging;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices.Tests;

internal class LoggingListener : ILoggerFactory, IDisposable
{
    public event Action<ILogger>? LoggerCreated;
    public event Action<LoggingListenerData<object?>>? Logged;
    public event Func<object?, IDisposable?>? BegunScope;
    public event Func<LogLevel, bool>? IsLevelEnabledCalled;

    public ILogger CreateLogger(string categoryName)
    {
        var logger = new CatchAllLogger(this, categoryName);
        LoggerCreated?.Invoke(logger);
        return logger;
    }

    public void AddProvider(ILoggerProvider provider) { }

    public void Dispose()
    {
        Logged = null;
        BegunScope = null;
    }

    private class CatchAllLogger(LoggingListener listener, string categoryName) : ILogger
    {
        public bool IsEnabled(LogLevel logLevel)
        {
            return listener.IsLevelEnabledCalled?.Invoke(logLevel)
                ?? true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            listener.Logged?.Invoke(new(
                CategoryName: categoryName,
                LogLevel: logLevel,
                EventId: eventId,
                State: state,
                Exception: exception,
                Message: formatter(state, exception)
            ));
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return listener.BegunScope?.Invoke(state);
        }
    }
}

internal record LoggingListenerData<TState>(string CategoryName, LogLevel LogLevel, EventId EventId, TState State, Exception? Exception, string Message);
