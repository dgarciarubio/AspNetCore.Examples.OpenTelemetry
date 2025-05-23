using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public class Telemetry : ITelemetry, IDisposable
{
    private bool _disposedValue = false;

    public Telemetry(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions options)
    : this(
        CreateLogger(loggerFactory, options),
        CreateActivitySource(options),
        CreateMeter(meterFactory, options)
    )
    {
    }

    private protected Telemetry(ILogger logger, ActivitySource activitySource, Meter meter)
    {
        Logger = logger;
        ActivitySource = activitySource;
        Meter = meter;
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

    private protected static ILogger CreateLogger(ILoggerFactory loggerFactory, TelemetryOptions options)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(options);
        return loggerFactory.CreateLogger(options.Name);
    }

    private protected static ActivitySource CreateActivitySource(TelemetryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return new ActivitySource(options.Name, options.Version, options.Tags);
    }

    private protected static Meter CreateMeter(IMeterFactory meterFactory, TelemetryOptions options)
    {
        ArgumentNullException.ThrowIfNull(meterFactory);
        ArgumentNullException.ThrowIfNull(options);
        return meterFactory.Create(new MeterOptions(options.Name)
        {
            Version = options.Version,
            Tags = options.Tags,
            Scope = options.Scope
        });
    }
}


public class Telemetry<TCategoryName> : Telemetry, ITelemetry<TCategoryName>
{
    public static readonly string Name = TelemetryOptions<TCategoryName>.Name;

    private static readonly TelemetryOptions<TCategoryName> DefaultOptions = new();

    public Telemetry(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions<TCategoryName>? options = null)
    : base(
        CreateLogger(loggerFactory),
        CreateActivitySource(options ?? DefaultOptions),
        CreateMeter(meterFactory, options ?? DefaultOptions)
    )
    {
        Logger = (ILogger<TCategoryName>)base.Logger;
    }

    public new ILogger<TCategoryName> Logger { get; }

    private static ILogger<TCategoryName> CreateLogger(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        return loggerFactory.CreateLogger<TCategoryName>();
    }
}