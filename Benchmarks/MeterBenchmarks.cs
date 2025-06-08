using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices.Benchmarks;

[MemoryDiagnoser]
public class MeterBenchmarks : IDisposable
{
    private static readonly Meter _staticMeter = new(nameof(StaticMeter));
    private static readonly Counter<int> _staticCounter = _staticMeter.CreateCounter<int>(nameof(StaticMeter));

    private readonly ServiceProvider _serviceProvider;

    private readonly Counter<int> _genericTelemetryCounter;
    private readonly Counter<int> _namedTelemetryCounter;
    private readonly MeterListener _listener;

    public MeterBenchmarks()
    {
        _serviceProvider = new ServiceCollection()
            .AddTelemetry(telemetry =>
            {
                telemetry.Add<NamedTelemetryMeter>(nameof(NamedTelemetryMeter)).Configure(o =>
                {
                    o.Version = "1.0";
                    o.Tags = new()
                    {
                        ["Tag1"] = "Value1",
                        ["Tag2"] = "Value2",
                    };
                });
                telemetry.Add<GenericTelemetryMeter>().Configure(o =>
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

        _genericTelemetryCounter = _serviceProvider.GetRequiredService<GenericTelemetryMeter>().Counter;
        _namedTelemetryCounter = _serviceProvider.GetRequiredService<NamedTelemetryMeter>().Counter;

        _listener = new()
        {
            InstrumentPublished = (instrument, listener) => listener.EnableMeasurementEvents(instrument),
        };
        _listener.SetMeasurementEventCallback<int>((_, _, _, _) => { });
        _listener.Start();
    }

    [Benchmark]
    public void MeterStatic()
    {
        _staticCounter.Add(1);
    }

    [Benchmark]
    public void MeterGeneric()
    {
        _genericTelemetryCounter.Add(1);
    }

    [Benchmark]
    public void MeterNamed()
    {
        _namedTelemetryCounter.Add(1);
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        _listener.Dispose();
        GC.SuppressFinalize(this);
    }

    private class StaticMeter { }

    private class GenericTelemetryMeter : Telemetry<GenericTelemetryMeter>
    {
        public GenericTelemetryMeter(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions<GenericTelemetryMeter> options)
            : base(loggerFactory, meterFactory, options)
        {
            Counter = Meter.CreateCounter<int>(nameof(Counter));
        }

        public Counter<int> Counter { get; }
    }

    private class NamedTelemetryMeter : Telemetry
    {
        public NamedTelemetryMeter(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions<NamedTelemetryMeter> options)
            : base(loggerFactory, meterFactory, options)
        {
            Counter = Meter.CreateCounter<int>(nameof(Counter));
        }

        public Counter<int> Counter { get; }
    }
}
