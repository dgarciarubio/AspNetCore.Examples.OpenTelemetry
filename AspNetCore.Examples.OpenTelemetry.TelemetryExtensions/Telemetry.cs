using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public class Telemetry : ITelemetry, IDisposable
{
    private bool _disposedValue = false;

    private readonly List<IDisposable?> _loggerScopes;

    public Telemetry(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions options)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));
        ArgumentNullException.ThrowIfNull(meterFactory, nameof(meterFactory));
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        _loggerScopes = [];
        Logger = loggerFactory.CreateLogger(options.Name);
        ActivitySource = new ActivitySource(
            name: options.Name,
            version: options.Version,
            tags: options.Tags
        );
        Meter = meterFactory.Create(new MeterOptions(options.Name)
        {
            Version = options.Version,
            Tags = options.Tags,
            Scope = options.Scope
        });

        BeginLoggerScopes(options);
    }

    public ILogger Logger { get; }
    public ActivitySource ActivitySource { get; }
    public Meter Meter { get; }

    protected virtual void Dispose(bool disposing)
    {
        if (Interlocked.Exchange(ref _disposedValue, true) == false)
        {
            if (disposing)
            {
                foreach (var scope in _loggerScopes)
                {
                    scope?.Dispose();
                }
                ActivitySource.Dispose();
                Meter.Dispose();
            }
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void BeginLoggerScopes(TelemetryOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.Version))
        {
            var versionTag = new KeyValuePair<string, object?>(nameof(options.Version), options.Version);
            _loggerScopes.Add(Logger.BeginScope(new[] { versionTag }));
        }
        if (options.Tags is not null && options.Tags.Any())
        {
            _loggerScopes.Add(Logger.BeginScope(options.Tags));
        }
        if (options.Scope is not null)
        {
            _loggerScopes.Add(Logger.BeginScope(options.Scope));
        }
    }
}

public class Telemetry<TTelemetryName>(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions<TTelemetryName>? options = null)
    : Telemetry(new GenericLoggerFactory(loggerFactory), meterFactory, options ?? DefaultOptions), ITelemetry<TTelemetryName>
{
    public static readonly string Name = TelemetryOptions<TTelemetryName>.Name;

    private static readonly TelemetryOptions<TTelemetryName> DefaultOptions = new();

    public new ILogger<TTelemetryName> Logger => (ILogger<TTelemetryName>)base.Logger;

    private sealed class GenericLoggerFactory(ILoggerFactory loggerFactory) : ILoggerFactory
    {
        private readonly ILoggerFactory _inner = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

        public void AddProvider(ILoggerProvider provider) => _inner.AddProvider(provider);

        public ILogger CreateLogger(string categoryName) => _inner.CreateLogger<TTelemetryName>();

        public void Dispose() => _inner.Dispose();
    }
}