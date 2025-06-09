using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices.Benchmarks;

[MemoryDiagnoser]
public class DependencyInjectionBenchmarks : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public DependencyInjectionBenchmarks()
    {
        _serviceProvider = new ServiceCollection()
            .AddTelemetry(telemetry =>
            {
                telemetry.AddFor("Name");
                telemetry.AddFor<TelemetryName>();
                telemetry.Add<TelemetryService>("NamedService");
                telemetry.Add<TelemetryOfTNameService>();
            })
            .BuildServiceProvider();
    }

    [Benchmark]
    public void ResolveLogger()
    {
        _serviceProvider.GetRequiredService<ILogger<UnregisteredTelemetryName>>();
    }

    [Benchmark]
    public void ResolveMeter()
    {
        _serviceProvider.GetRequiredService<IMeterFactory>();
    }

    [Benchmark]
    public void ResolveNamed()
    {
        _serviceProvider.GetRequiredKeyedService<ITelemetry>("Name");
    }

    [Benchmark]
    public void ResolveTelemetryOfUnregisteredName()
    {
        _serviceProvider.GetRequiredService<ITelemetry<UnregisteredTelemetryName>>();
    }

    [Benchmark]
    public void ResolveTelemetryOfTName()
    {
        _serviceProvider.GetRequiredService<ITelemetry<TelemetryName>>();
    }

    [Benchmark]
    public void ResolveTelemetryService()
    {
        _serviceProvider.GetRequiredService<TelemetryService>();
    }

    [Benchmark]
    public void ResolveTelemetryOfTNameService()
    {
        _serviceProvider.GetRequiredService<TelemetryOfTNameService>();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }

    private class UnregisteredTelemetryName { }

    private class TelemetryName { }

    private class TelemetryService(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions<TelemetryService> options) 
        : Telemetry(loggerFactory, meterFactory, options)
    {
    }

    private class TelemetryOfTNameService(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions<TelemetryOfTNameService> options)
        : Telemetry<TelemetryName>(loggerFactory, meterFactory, options)
    {
    }
}
