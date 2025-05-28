using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class Extensions
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services, string name)
    {
        return services
            .AddKeyedSingleton<ITelemetry>(name, (sp, _) =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var meterFactory = sp.GetRequiredService<IMeterFactory>();
                var telemetryOptions = new TelemetryOptions(name);
                return new Telemetry(loggerFactory, meterFactory, telemetryOptions);
            })
            .ConfigureOpenTelemetryTracerProvider(t => t.AddSource(name))
            .ConfigureOpenTelemetryMeterProvider(m => m.AddMeter(name));
    }

    public static IServiceCollection AddTelemetry(this IServiceCollection services, TelemetryOptions telemetryOptions)
    {
        ArgumentNullException.ThrowIfNull(telemetryOptions, nameof(telemetryOptions));
        var name = telemetryOptions.Name;

        return services
            .AddKeyedSingleton<ITelemetry>(name, (sp, _) =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var meterFactory = sp.GetRequiredService<IMeterFactory>();
                return new Telemetry(loggerFactory, meterFactory, telemetryOptions);
            })
            .ConfigureOpenTelemetryTracerProvider(t => t.AddSource(name))
            .ConfigureOpenTelemetryMeterProvider(m => m.AddMeter(name));
    }

    public static IServiceCollection AddTelemetry<TTelemetryName>(this IServiceCollection services, TelemetryOptions<TTelemetryName>? telemetryOptions = null)
    {
        var name = TelemetryOptions<TTelemetryName>.Name;

        return services
            .AddSingleton<ITelemetry<TTelemetryName>>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var meterFactory = sp.GetRequiredService<IMeterFactory>();
                return new Telemetry<TTelemetryName>(loggerFactory, meterFactory, telemetryOptions);
            })
            .AddKeyedSingleton(name, (sp, _) => sp.GetRequiredService<ITelemetry<TTelemetryName>>())
            .ConfigureOpenTelemetryTracerProvider(t => t.AddSource(name))
            .ConfigureOpenTelemetryMeterProvider(m => m.AddMeter(name));
    }
}