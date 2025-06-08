using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices.Benchmarks;

[MemoryDiagnoser]
public class ActivitySourceBenchmarks : IDisposable
{
    private static readonly ActivitySource _staticActivitySource = new ActivitySource(nameof(StaticActivitySource));

    private readonly ServiceProvider _serviceProvider;

    private readonly ActivitySource _genericTelemetryActivitySource;
    private readonly ActivitySource _namedTelemetryActivitySource;
    private readonly ActivityListener _listener;

    public ActivitySourceBenchmarks()
    {
        _serviceProvider = new ServiceCollection()
            .AddTelemetry(telemetry =>
            {
                telemetry.For(nameof(NamedTelemetryActivitySource)).Configure(o =>
                {
                    o.Version = "1.0";
                    o.Tags = new()
                    {
                        ["Tag1"] = "Value1",
                        ["Tag2"] = "Value2",
                    };
                });
                telemetry.For<GenericTelemetryActivitySource>().Configure(o =>
                {
                    o.Version = "1.0";
                    o.Tags = new()
                    {
                        ["Tag1"] = "Value1",
                        ["Tag2"] = "Value2",
                    };
                });
            })
            .BuildServiceProvider();

        _genericTelemetryActivitySource = _serviceProvider.GetRequiredService<ITelemetry<GenericTelemetryActivitySource>>().ActivitySource;
        _namedTelemetryActivitySource = _serviceProvider.GetRequiredKeyedService<ITelemetry>(nameof(NamedTelemetryActivitySource)).ActivitySource;

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
        using var activity = _staticActivitySource.StartActivity(nameof(StaticActivitySource));
    }

    [Benchmark]
    public void ActivityGeneric()
    {
        using var activity = _genericTelemetryActivitySource.StartActivity(nameof(GenericTelemetryActivitySource));
    }

    [Benchmark]
    public void ActivityNamed()
    {
        using var activity = _namedTelemetryActivitySource.StartActivity(nameof(NamedTelemetryActivitySource));
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        _listener.Dispose();
    }

    private class StaticActivitySource { }

    private class GenericTelemetryActivitySource { }

    private class NamedTelemetryActivitySource { }
}
