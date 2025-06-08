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

        var logger = loggerFactory.CreateLogger(options.Name);
        Logger = OptionsEnrichedLogger.Create(logger, options);
        ActivitySource = new ActivitySource(options.Name,
            version: options.Version,
            tags: options.Tags
        );
        Meter = meterFactory.Create(options.Name,
            version: options.Version,
            tags: options.Tags
        );
        Options = options;
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
        Logger = OptionsEnrichedLogger<TTelemetryName>.Create(logger, Options);
    }

    public new ILogger<TTelemetryName> Logger { get; }

    private static TelemetryOptions SanitizeName(TelemetryOptions? options)
    {
        if (options is not null && options.Name != Name)
        {
            throw new ArgumentException("The supplied telemetry options do not have the expected name.", nameof(options));
        }
        return new TelemetryOptions
        {
            Name = Name,
            Tags = options?.Tags,
            Version = options?.Version,
        };
    }
}