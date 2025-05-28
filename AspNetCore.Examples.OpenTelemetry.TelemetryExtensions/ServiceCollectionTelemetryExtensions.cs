using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;
using System.Xml.Linq;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class ServiceCollectionTelemetryExtensions
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services, string name)
    {
        return services.AddTelemetry(new TelemetryOptions(name));
    }

    public static IServiceCollection AddTelemetry(this IServiceCollection services, TelemetryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        var name = options.Name;

        return services
            .AddKeyedSingleton<ITelemetry>(name, (sp, _) =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var meterFactory = sp.GetRequiredService<IMeterFactory>();
                return new Telemetry(loggerFactory, meterFactory, options);
            })
            .ConfigureOpenTelemetryProviders(name);
    }

    public static IServiceCollection AddTelemetry<TTelemetryName>(this IServiceCollection services)
    {
        return services.AddTelemetry<TTelemetryName>(options: null);
    }

    public static IServiceCollection AddTelemetry<TTelemetryName>(this IServiceCollection services, TelemetryOptions<TTelemetryName>? options)
    {
        var name = TelemetryOptions<TTelemetryName>.Name;

        return services
            .AddSingleton<ITelemetry<TTelemetryName>>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var meterFactory = sp.GetRequiredService<IMeterFactory>();
                return new Telemetry<TTelemetryName>(loggerFactory, meterFactory, options);
            })
            .AddKeyedSingleton<ITelemetry>(name, (sp, _) => sp.GetRequiredService<ITelemetry<TTelemetryName>>())
            .ConfigureOpenTelemetryProviders(name);
    }

    private static IServiceCollection ConfigureOpenTelemetryProviders(this IServiceCollection services, string name)
    {
        return services
           .ConfigureOpenTelemetryTracerProvider(t => t.AddSource(name))
           .ConfigureOpenTelemetryMeterProvider(m => m.AddMeter(name));
    }
}