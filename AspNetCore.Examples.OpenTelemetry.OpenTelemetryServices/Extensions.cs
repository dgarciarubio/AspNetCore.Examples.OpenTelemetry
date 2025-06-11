using AspNetCore.Examples.OpenTelemetry.TelemetryServices;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class TelemetryServiceBuilderExtensions
{
    public static TelemetryServiceBuilder AddToOpenTelemetryProviders(this TelemetryServiceBuilder builder, bool addSource = true, bool addMeter = true)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
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

public static class OpenTelemetryProviderBuilderExtensions
{
    public static TracerProviderBuilder AddSourceFor<TTelemetryName>(this TracerProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        var name = Telemetry<TTelemetryName>.Name;
        return builder.AddSource(name);
    }

    public static MeterProviderBuilder AddMeterFor<TTelemetryName>(this MeterProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        var name = Telemetry<TTelemetryName>.Name;
        return builder.AddMeter(name);
    }
}
