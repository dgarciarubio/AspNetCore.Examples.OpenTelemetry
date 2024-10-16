using AspNetCore.Examples.OpenTelemetry.TelemetryExtensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public class Telemetry<TCategoryName> : ITelemetry<TCategoryName>, IDisposable
{
    private bool disposedValue;

    public static readonly string CategoryName = TypeNameHelper.GetTypeDisplayName(typeof(TCategoryName), includeGenericParameters: false, nestedTypeDelimiter: '.');

    private Lazy<ILogger<TCategoryName>>? _logger;
    private Lazy<ActivitySource>? _activitySource;
    private Lazy<Meter>? _meter;

    public Telemetry(ILoggerFactory loggerFactory, IMeterFactory meterFactory)
    {
        _logger = new Lazy<ILogger<TCategoryName>>(() => 
            loggerFactory.CreateLogger<TCategoryName>(), 
            isThreadSafe: true);
        _activitySource = new Lazy<ActivitySource>(() => 
            new ActivitySource(CategoryName, ActivitySourceOptions.Version, ActivitySourceOptions.Tags),
            isThreadSafe: true);
        _meter = new Lazy<Meter>(() => 
            meterFactory.Create(new MeterOptions(CategoryName)
            {
                Version = MeterOptions.Version,
                Tags = MeterOptions.Tags,
                Scope = MeterOptions.Scope,
            }),
            isThreadSafe: true);
    }


    public ILogger<TCategoryName> Logger => _logger?.Value ?? throw new ObjectDisposedException(nameof(Logger));
    public ActivitySource ActivitySource => _activitySource?.Value ?? throw new ObjectDisposedException(nameof(ActivitySource));
    public Meter Meter => _meter?.Value ?? throw new ObjectDisposedException(nameof(Meter));

    protected virtual ActivitySourceOptions ActivitySourceOptions => new(CategoryName);
    protected virtual MeterOptions MeterOptions => new(CategoryName);

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Interlocked.Exchange(ref _logger, null);

                var activitySource = Interlocked.Exchange(ref _activitySource, null);
                if (activitySource?.IsValueCreated ?? false)
                {
                    activitySource.Value.Dispose();
                }

                var meter = Interlocked.Exchange(ref _meter, null);
                if (meter?.IsValueCreated ?? false)
                {
                    meter.Value.Dispose();
                }
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
