using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public class Telemetry : ITelemetry, IDisposable
{
    private bool _disposedValue = false;

    public Telemetry(ILoggerFactory loggerFactory, IMeterFactory meterFactory, string name, TelemetryOptions? options = null)
    : this(
        CreateLogger(loggerFactory, name),
        CreateActivitySource(name, options),
        CreateMeter(meterFactory, name, options)
    )
    { }

    private protected Telemetry(ILogger logger, ActivitySource activitySource, Meter meter)
    {
        Logger = logger;
        ActivitySource = activitySource;
        Meter = meter;
        meter.CreateCounter<int>("instances").Add(1);
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

    private protected static ILogger CreateLogger(ILoggerFactory loggerFactory, string name)
    {
        return loggerFactory.CreateLogger(name);
    }

    private protected static ActivitySource CreateActivitySource(string name, TelemetryOptions? options = null)
    {
        return new ActivitySource(name, options?.Version, options?.Tags);
    }

    private protected static Meter CreateMeter(IMeterFactory meterFactory, string name, TelemetryOptions? options = null)
    {
        return meterFactory.Create(new MeterOptions(name)
        {
            Version = options?.Version,
            Tags = options?.Tags,
            Scope = options?.Scope,
        });
    }
}


public class Telemetry<TCategoryName> : Telemetry, ITelemetry<TCategoryName>
{
    public Telemetry(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions? options = null)
    : base(
        CreateLogger(loggerFactory, out var logger, out var name),
        CreateActivitySource(name, options),
        CreateMeter(meterFactory, name, options)
    )
    {
        Logger = logger;
    }

    public new ILogger<TCategoryName> Logger { get; }

    private static ILogger CreateLogger(ILoggerFactory loggerFactory, out ILogger<TCategoryName> logger, out string categoryName)
    {
        var observer = new LoggerFactoryCategoryNameObserver(loggerFactory);
        logger = observer.CreateLogger<TCategoryName>();
        categoryName = observer.CategoryName ?? throw new InvalidOperationException("Could not retrieve category name from generic logger.");
        return logger;
    }

    private class LoggerFactoryCategoryNameObserver(ILoggerFactory inner) : ILoggerFactory
    {
        public string? CategoryName { get; private set; }

        public ILogger CreateLogger(string categoryName)
        {
            CategoryName = categoryName;
            return inner.CreateLogger(categoryName);
        }

        public void AddProvider(ILoggerProvider provider) => inner.AddProvider(provider);

        public void Dispose() => inner.Dispose();
    }
}
