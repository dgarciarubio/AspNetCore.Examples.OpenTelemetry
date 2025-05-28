using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public interface ITelemetry
{
    ILogger Logger { get; }
    ActivitySource ActivitySource { get; }
    Meter Meter { get; }
}

public interface ITelemetry<out TTelemetryName> : ITelemetry
{
    new ILogger<TTelemetryName> Logger { get; }
}