using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public interface ITelemetry<out TCategoryName> 
{
    ILogger<TCategoryName> Logger { get; }
    ActivitySource ActivitySource { get; }
    Meter Meter { get; }
}