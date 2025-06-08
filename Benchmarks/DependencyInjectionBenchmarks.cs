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
                telemetry.For("Name");
                telemetry.For<TelemetryName>();
                telemetry.Add<NamedTelemetryService>("NamedService");
                telemetry.Add<GenericTelemetryService>();
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
        _serviceProvider.GetRequiredService<ILogger<IMeterFactory>>();
    }

    [Benchmark]
    public void ResolveNamed()
    {
        _serviceProvider.GetRequiredKeyedService<ITelemetry>("Name");
    }

    [Benchmark]
    public void ResolveUnregistered()
    {
        _serviceProvider.GetRequiredService<ITelemetry<UnregisteredTelemetryName>>();
    }

    [Benchmark]
    public void ResolveGeneric()
    {
        _serviceProvider.GetRequiredService<ITelemetry<TelemetryName>>();
    }

    [Benchmark]
    public void ResolveService()
    {
        _serviceProvider.GetRequiredService<NamedTelemetryService>();
    }

    [Benchmark]
    public void ResolveGenericService()
    {
        _serviceProvider.GetRequiredService<GenericTelemetryService>();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    private class UnregisteredTelemetryName { }

    private class TelemetryName { }

    private class NamedTelemetryService : Telemetry
    {
        public NamedTelemetryService(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions<NamedTelemetryService> options)
            : base(loggerFactory, meterFactory, options)
        {
        }
    }

    private class GenericTelemetryService : Telemetry<TelemetryName>
    {
        public GenericTelemetryService(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions<GenericTelemetryService> options)
            : base(loggerFactory, meterFactory, options)
        {
        }
    }
}
