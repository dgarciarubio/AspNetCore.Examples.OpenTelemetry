using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace System.Diagnostics;

public interface ITelemetry<out TCategoryName> 
{
    ILogger<TCategoryName> Logger { get; }
    ActivitySource ActivitySource { get; }
    Meter Meter { get; }
}