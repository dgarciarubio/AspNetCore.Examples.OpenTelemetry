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

    private readonly Counter<int> _telemetryOfTName;
    private readonly Counter<int> _namedTelemetry;
    private readonly MeterListener _listener;

    public MeterBenchmarks()
    {
        _serviceProvider = new ServiceCollection()
            .AddTelemetry(telemetry =>
            {
                telemetry.Add<TelemetryService>("Name", o =>
                {
                    o.Version = "1.0";
                    o.Tags = new Dictionary<string, object?>()
                    {
                        ["Tag1"] = "Value1",
                        ["Tag2"] = "Value2",
                    };
                });
                telemetry.Add<TelemetryOfTNameService>(o =>
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

        _telemetryOfTName = _serviceProvider.GetRequiredService<TelemetryOfTNameService>().Counter;
        _namedTelemetry = _serviceProvider.GetRequiredService<TelemetryService>().Counter;

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
    public void MeterOfTName()
    {
        _telemetryOfTName.Add(1);
    }

    [Benchmark]
    public void MeterNamed()
    {
        _namedTelemetry.Add(1);
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        _listener.Dispose();
        GC.SuppressFinalize(this);
    }

    private class StaticMeter { }

    private class TelemetryOfTNameService : Telemetry<TelemetryOfTNameService>
    {
        public TelemetryOfTNameService(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions<TelemetryOfTNameService> options)
            : base(loggerFactory, meterFactory, options)
        {
            Counter = Meter.CreateCounter<int>(nameof(Counter));
        }

        public Counter<int> Counter { get; }
    }

    private class TelemetryService : Telemetry
    {
        public TelemetryService(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions<TelemetryService> options)
            : base(loggerFactory, meterFactory, options)
        {
            Counter = Meter.CreateCounter<int>(nameof(Counter));
        }

        public Counter<int> Counter { get; }
    }
}
