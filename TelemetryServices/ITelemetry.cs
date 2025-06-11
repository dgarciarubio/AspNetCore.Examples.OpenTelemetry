using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace TelemetryServices;

public interface ITelemetry : IDisposable
{
    ILogger Logger { get; }
    ActivitySource ActivitySource { get; }
    Meter Meter { get; }
}

public interface ITelemetry<out TTelemetryName> : ITelemetry
{
    new ILogger<TTelemetryName> Logger { get; }
}
