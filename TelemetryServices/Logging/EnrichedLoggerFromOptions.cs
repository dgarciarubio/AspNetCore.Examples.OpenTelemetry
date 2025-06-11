using Microsoft.Extensions.Logging;

namespace TelemetryServices.Logging;

internal class EnrichedLoggerFromOptions : ILogger
{
    private readonly ILogger _inner;
    private readonly IReadOnlyList<KeyValuePair<string, object?>> _options;

    protected EnrichedLoggerFromOptions(ILogger inner, IReadOnlyList<KeyValuePair<string, object?>> options)
    {
        _inner = inner;
        _options = options;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _inner.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => _inner.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var enrichedState = new EnrichedLogState<TState>(_options, state);
        _inner.Log(logLevel, eventId, enrichedState, exception, (s, e) => formatter(s.OriginalState, e));
    }

    public static ILogger Create(ILogger inner, TelemetryElementOptions options)
    {
        var structuredStateOptions = AsStructuredState(options).ToArray();
        return structuredStateOptions switch
        {
            { Length: 0 } => inner,
            _ => new EnrichedLoggerFromOptions(inner, structuredStateOptions),
        };
    }

    protected static IEnumerable<KeyValuePair<string, object?>> AsStructuredState(TelemetryElementOptions options)
    {
        if (!string.IsNullOrEmpty(options.Version))
        {
            yield return new(nameof(options.Version), options.Version);
        }
        if (options.Tags is not null)
        {
            foreach (var tag in options.Tags)
            {
                yield return tag;
            }
        }
    }
}

internal class EnrichedLoggerFromOptions<TCategoryName> : EnrichedLoggerFromOptions, ILogger<TCategoryName>
{
    private EnrichedLoggerFromOptions(ILogger<TCategoryName> inner, KeyValuePair<string, object?>[] optionsState)
        : base(inner, optionsState)
    {
    }

    public static ILogger<TCategoryName> Create(ILogger<TCategoryName> inner, TelemetryElementOptions options)
    {
        var optionsState = AsStructuredState(options).ToArray();
        return optionsState switch
        {
            { Length: 0 } => inner,
            _ => new EnrichedLoggerFromOptions<TCategoryName>(inner, optionsState),
        };
    }
}
