using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using TelemetryServices.OpenTelemetry.Tests.TestDoubles;

namespace TelemetryServices.OpenTelemetry.Tests;

public sealed class TelemetryServiceBuilderExtensions_should : IDisposable
{
    private readonly HostApplicationBuilder _hostBuilder;
    private readonly IServiceCollection _services;
    private readonly OpenTelemetryTraceListener _traceListener;
    private readonly OpenTelemetryMetricListener _metricListener;

    public TelemetryServiceBuilderExtensions_should()
    {
        _hostBuilder = Host.CreateApplicationBuilder();
        _services = _hostBuilder.Services;
        _traceListener = new OpenTelemetryTraceListener();
        _metricListener = new OpenTelemetryMetricListener();
        _services.AddOpenTelemetry()
            .WithTracing(t => t.AddProcessor(_traceListener))
            .WithMetrics(m => m.AddReader(_metricListener));
    }

    [Fact]
    public void Fail_if_null_builder()
    {
        var action = () => TelemetryServiceBuilderExtensions.AddToOpenTelemetryProviders(builder: null!);

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal("builder", exception.ParamName);
    }

    [Fact]
    public void Add_activity_source_to_open_telemetry_provider()
    {
        _services.AddTelemetryFor<TelemetryName>()
            .AddToOpenTelemetryProviders(addSource: true, addMeter: false);

        _hostBuilder.RunInHost(host =>
        {
            var telemetry = host.Services.GetRequiredService<ITelemetry<TelemetryName>>();
            using var activity = telemetry.ActivitySource.StartActivity("TestActivity");
            var recordedActivities = _traceListener.Activities;
            Assert.NotNull(activity);
            Assert.Contains(recordedActivities, a => a.DisplayName == activity.DisplayName);
        });
    }

    [Fact]
    public async Task Add_meter_to_open_telemetry_provider()
    {
        _services.AddTelemetryFor<TelemetryName>()
            .AddToOpenTelemetryProviders(addSource: false, addMeter: true);

        await _hostBuilder.RunInHost(async host =>
        {
            var telemetry = host.Services.GetRequiredService<ITelemetry<TelemetryName>>();
            var meter = telemetry.Meter.CreateCounter<int>("TestCounter");
            meter.Add(1);
            var recordedMetrics = await _metricListener.WaitForMetrics();
            Assert.Contains(recordedMetrics, m => m.Name == meter.Name);
        });
    }

    public void Dispose()
    {
        _traceListener.Dispose();
        _metricListener.Dispose();
    }

    private class TelemetryName { }
}
