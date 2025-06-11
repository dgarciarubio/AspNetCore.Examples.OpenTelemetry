using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace TelemetryServices.Benchmarks;

[MemoryDiagnoser]
public class ActivitySourceBenchmarks : IDisposable
{
    private static readonly ActivitySource _staticSource = new("Static");

    private readonly ServiceProvider _serviceProvider;

    private readonly ActivitySource _telemetrySource;
    private readonly ActivitySource _telemetryOfTNameSource;
    private readonly ActivityListener _listener;

    public ActivitySourceBenchmarks()
    {
        _serviceProvider = new ServiceCollection()
            .AddTelemetry(telemetry =>
            {
                telemetry.AddFor("Name", o =>
                {
                    o.Version = "1.0";
                    o.Tags = new Dictionary<string, object?>()
                    {
                        ["Tag1"] = "Value1",
                        ["Tag2"] = "Value2",
                    };
                });
                telemetry.AddFor<TelemetryName>(o =>
                {
                    o.Version = "1.0";
                    o.Tags = new Dictionary<string, object?>()
                    {
                        ["Tag1"] = "Value1",
                        ["Tag2"] = "Value2",
                    };
                });
            })
            .BuildServiceProvider();

        _telemetrySource = _serviceProvider.GetRequiredKeyedService<ITelemetry>("Name").ActivitySource;
        _telemetryOfTNameSource = _serviceProvider.GetRequiredService<ITelemetry<TelemetryName>>().ActivitySource;

        _listener = new()
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = activity => { },
            ActivityStopped = activity => { },
        };
        ActivitySource.AddActivityListener(_listener);
    }

    [Benchmark]
    public void ActivityStatic()
    {
        using var activity = _staticSource.StartActivity("Activity");
    }

    [Benchmark]
    public void ActivityOfTName()
    {
        using var activity = _telemetryOfTNameSource.StartActivity("Activity");
    }

    [Benchmark]
    public void ActivityNamed()
    {
        using var activity = _telemetrySource.StartActivity("Activity");
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        _listener.Dispose();
        GC.SuppressFinalize(this);
    }

    private class TelemetryName { }
}
