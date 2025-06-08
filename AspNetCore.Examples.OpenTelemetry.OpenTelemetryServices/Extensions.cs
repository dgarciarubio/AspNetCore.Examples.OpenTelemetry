using AspNetCore.Examples.OpenTelemetry.TelemetryServices;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class OpenTelemetryProviderBuilderExtensions
{
    public static TracerProviderBuilder AddSourceFor<TTelemetryName>(this TracerProviderBuilder traces)
    {
        var name = Telemetry<TTelemetryName>.Name;
        return traces.AddSource(name);
    }

    public static MeterProviderBuilder AddMeterFor<TTelemetryName>(this MeterProviderBuilder meters)
    {
        var name = Telemetry<TTelemetryName>.Name;
        return meters.AddMeter(name);
    }
}

public static class TelemetryServicesBuilderExtensions
{
    public static TelemetryServiceBuilder AddToOpenTelemetryProviders(this TelemetryServiceBuilder builder, bool addSource = true, bool addMeter = true)
    {
        if (addSource)
        {
            builder.Services.ConfigureOpenTelemetryTracerProvider(t => t.AddSource(builder.Name));
        }
        if (addMeter)
        {
            builder.Services.ConfigureOpenTelemetryMeterProvider(m => m.AddMeter(builder.Name));
        }
        return builder;
    }
}



