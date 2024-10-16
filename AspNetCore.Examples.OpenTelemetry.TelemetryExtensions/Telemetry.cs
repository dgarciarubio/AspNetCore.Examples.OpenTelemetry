using AspNetCore.Examples.OpenTelemetry.TelemetryExtensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace System.Diagnostics;

public class Telemetry<TCategoryName> : ITelemetry<TCategoryName>
{
    private bool disposedValue;

    public static readonly string CategoryName = TypeNameHelper.GetTypeDisplayName(typeof(TCategoryName), includeGenericParameters: false, nestedTypeDelimiter: '.');

    public Telemetry(ILoggerFactory loggerFactory, IMeterFactory meterFactory)
    {
        Logger = loggerFactory.CreateLogger<TCategoryName>();
        ActivitySource = new ActivitySource(ActivitySourceOptions.Name, ActivitySourceOptions.Version, ActivitySourceOptions.Tags);
        Meter = meterFactory.Create(MeterOptions);
    }

    public ILogger<TCategoryName> Logger { get; }
    public ActivitySource ActivitySource { get; }
    public Meter Meter { get; }

    protected virtual ActivitySourceOptions ActivitySourceOptions => new ActivitySourceOptions(CategoryName);
    protected virtual MeterOptions MeterOptions => new MeterOptions(CategoryName);

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                ActivitySource.Dispose();
                Meter.Dispose();
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
