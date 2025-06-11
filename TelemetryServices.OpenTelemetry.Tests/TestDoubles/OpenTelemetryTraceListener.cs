using OpenTelemetry;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace TelemetryServices.OpenTelemetry.Tests.TestDoubles;

internal class OpenTelemetryTraceListener : BaseProcessor<Activity>
{
    public ConcurrentQueue<Activity> Activities { get; } = [];

    public override void OnStart(Activity data)
    {
        Activities.Enqueue(data);
    }
}
