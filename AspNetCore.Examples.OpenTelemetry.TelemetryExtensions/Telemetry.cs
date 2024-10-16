using AspNetCore.Examples.OpenTelemetry.TelemetryExtensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace System.Diagnostics;

public class Telemetry<TCategoryName> : ITelemetry<TCategoryName>
{
    private bool disposedValue;

    internal static readonly string CategoryName = TypeNameHelper.GetTypeDisplayName(typeof(TCategoryName), includeGenericParameters: false, nestedTypeDelimiter: '.');

    public Telemetry(ILoggerFactory loggerFactory, IMeterFactory meterFactory)
    {
        Logger = loggerFactory.CreateLogger<TCategoryName>();
        ActivitySource = new ActivitySource(CategoryName, Version);
        Meter = meterFactory.Create(new MeterOptions(CategoryName)
        {
            Version = Version,
            Scope = Scope,
            Tags = Tags,
        });
    }

    public ILogger<TCategoryName> Logger { get; }
    public ActivitySource ActivitySource { get; }
    public Meter Meter { get; }

    protected virtual string? Version { get; }
    protected virtual object? Scope { get; }
    protected virtual IEnumerable<KeyValuePair<string, object?>>? Tags { get; }

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