using AspNetCore.Examples.OpenTelemetry.TelemetryServices.Logging;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices;

public class Telemetry : ITelemetry, IDisposable
{
    private bool _disposedValue;

    public Telemetry(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions options)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));
        ArgumentNullException.ThrowIfNull(meterFactory, nameof(meterFactory));
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        Options = options;

        var logger = loggerFactory.CreateLogger(Options.Logger.Name);
        Logger = EnrichedLoggerFromOptions.Create(logger, Options.Logger);
        ActivitySource = new ActivitySource(Options.ActivitySource.Name,
            version: Options.ActivitySource.Version,
            tags: Options.ActivitySource.Tags
        );
        Meter = meterFactory.Create(Options.Meter.Name,
            version: Options.Meter.Version,
            tags: Options.Meter.Tags
        );
    }

    public ILogger Logger { get; }
    public ActivitySource ActivitySource { get; }
    public Meter Meter { get; }

    internal TelemetryOptions Options { get; }

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
}

public class Telemetry<TTelemetryName> : Telemetry, ITelemetry<TTelemetryName>, IDisposable
{
    public static string Name { get; } = TelemetryNameHelper.GetName<TTelemetryName>();

    public Telemetry(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions? options = null)
        : base(
            loggerFactory,
            meterFactory,
            SanitizeName(options)
        )
    {
        var logger = loggerFactory.CreateLogger<TTelemetryName>();
        Logger = EnrichedLoggerFromOptions<TTelemetryName>.Create(logger, Options.Logger);
    }

    public new ILogger<TTelemetryName> Logger { get; }

    private static TelemetryOptions SanitizeName(TelemetryOptions? options)
    {
        if (options is null)
        {
            return new TelemetryOptions { Name = Name };
        }
        if (options.Name != Name)
        {
            throw new ArgumentException("The supplied telemetry options do not have the expected name.", nameof(options));
        }
        return options;
    }
}