using Microsoft.Extensions.Logging;
using DefaultLogState = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object?>>;

namespace TelemetryServices.Tests.TestDoubles;

internal class LoggingListener : ILoggerFactory, IDisposable
{
    public LogLevel MinLogLevel { get; set; } = LogLevel.Trace;

    public List<ILogger> Loggers { get; } = [];

    public List<LoggingListenerData<object?>> Logs { get; } = [];
    public IEnumerable<LoggingListenerData<DefaultLogState>> FormattedLogs => Logs
        .Select(l => l.AsOf<DefaultLogState>()!)
        .Where(l => l is not null);

    public List<object?> Scopes { get; } = [];
    public IEnumerable<DefaultLogState> TagListScopes => Scopes.OfType<DefaultLogState>();

    public ILogger CreateLogger(string categoryName)
    {
        var logger = new CatchAllLogger(this, categoryName);
        Loggers.Add(logger);
        return logger;
    }

    public void AddProvider(ILoggerProvider provider) { }

    public void Dispose() { }

    private class CatchAllLogger(LoggingListener listener, string categoryName) : ILogger
    {
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= listener.MinLogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            listener.Logs.Add(new()
            {
                CategoryName = categoryName,
                LogLevel = logLevel,
                Message = formatter(state, exception),
                EventId = eventId,
                State = state,
                Exception = exception,
            });
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            listener.Scopes.Add(state);
            return NullScopeDisposable.Instance;
        }
    }

    public sealed class NullScopeDisposable : IDisposable
    {
        public static readonly NullScopeDisposable Instance = new();

        private NullScopeDisposable() { }

        public void Dispose() { }
    }
}

internal record LoggingListenerData<TState>()
{
    public required string CategoryName { get; init; }
    public required LogLevel LogLevel { get; init; }
    public required string Message { get; init; }
    public EventId EventId { get; init; }
    public TState? State { get; init; }
    public Exception? Exception { get; init; }

    public LoggingListenerData<TCastedState>? AsOf<TCastedState>()
    {
        if (State is null)
        {
            return new()
            {
                CategoryName = CategoryName,
                LogLevel = LogLevel,
                Message = Message,
                EventId = EventId,
                State = default,
                Exception = Exception,
            };
        }

        if (State is TCastedState castedState)
        {
            return new ()
            {
                CategoryName = CategoryName,
                LogLevel = LogLevel,
                Message = Message,
                EventId = EventId,
                State = castedState,
                Exception = Exception,
            };
        }

        return null;
    }
}
