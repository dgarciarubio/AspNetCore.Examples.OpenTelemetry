using AspNetCore.Examples.OpenTelemetry.TelemetryServices;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace OpenTelemetry.Trace
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public static class OpenTelemetryTracerProviderBuilderExtensions
    {
        public static TracerProviderBuilder AddSourceFor<TTelemetryName>(this TracerProviderBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            var name = Telemetry<TTelemetryName>.Name;
            return builder.AddSource(name);
        }
    }
}

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace OpenTelemetry.Metrics
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public static class OpenTelemetryMeterProviderBuilderExtensions
    {
        public static MeterProviderBuilder AddMeterFor<TTelemetryName>(this MeterProviderBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            var name = Telemetry<TTelemetryName>.Name;
            return builder.AddMeter(name);
        }
    }
}
