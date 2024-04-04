using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AspNetCore.Examples.OpenTelemetry.Api.Extensions;

public static class TelemetryExtensions
{
    public static IServiceCollection AddCustomOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var openTelemetryConfig = configuration.GetSection("OpenTelemetry");
        if (!openTelemetryConfig.Exists())
            return services;

        return services.AddOpenTelemetry()
            .ConfigureCustomResource(openTelemetryConfig)
            .WithCustomTracing(openTelemetryConfig)
            .WithCustomMetrics(openTelemetryConfig)
            .Services;
    }

    public static ILoggingBuilder AddCustomOpenTelemetry(this ILoggingBuilder logging, IConfiguration configuration)
    {
        var openTelemetryConfig = configuration.GetSection("OpenTelemetry");
        if (!openTelemetryConfig.Exists())
            return logging;

        var loggingConfig = openTelemetryConfig.GetSection("Logging");
        if (!loggingConfig.Exists())
            return logging;

        logging.AddOpenTelemetry(options =>
        {
            loggingConfig.Bind(options);

            var otlpExporterConfig = openTelemetryConfig.GetSection("Exporters:Otlp");
            var enableOtlpExporter = loggingConfig.GetValue("Exporters:Otlp", defaultValue: false);
            if (enableOtlpExporter && otlpExporterConfig.Exists())
            {
                options.AddOtlpExporter(otlpExporterConfig.Bind);
            }

            // Add more exporters as needed 
            // https://www.nuget.org/packages?q=opentelemetry.exporter
        });

        return logging;
    }

    private static IOpenTelemetryBuilder ConfigureCustomResource(this IOpenTelemetryBuilder builder, IConfigurationSection openTelemetryConfig)
    {
        var resourceConfig = openTelemetryConfig.GetSection("Resource");
        if (!resourceConfig.Exists())
            return builder;

        return builder.ConfigureResource(builder =>
        {
            var serviceOptions = resourceConfig.GetSection("Service").Get<ServiceOptions>();
            if (!string.IsNullOrWhiteSpace(serviceOptions?.Name))
            {
                builder.AddService(
                    serviceName: serviceOptions.Name,
                    serviceNamespace: serviceOptions.Namespace,
                    serviceVersion: serviceOptions.Version,
                    autoGenerateServiceInstanceId: serviceOptions.AutoGenerateInstanceId || string.IsNullOrWhiteSpace(serviceOptions.InstanceId),
                    serviceInstanceId: serviceOptions.AutoGenerateInstanceId ? null : serviceOptions.InstanceId
                );
            }
        });
    }

    private static IOpenTelemetryBuilder WithCustomTracing(this IOpenTelemetryBuilder builder, IConfigurationSection openTelemetryConfig)
    {
        var tracingConfig = openTelemetryConfig.GetSection("Tracing");
        if (!tracingConfig.Exists())
            return builder;

        return builder.WithTracing(builder =>
        {
            var sources = tracingConfig.GetSection("Sources").Get<string[]>();
            if (sources is not null)
            {
                builder.AddSource(sources);
            }

            var aspNetCoreInstrCongfig = tracingConfig.GetSection("Instrumentation:AspNetCore");
            if (aspNetCoreInstrCongfig.Exists())
            {
                builder.AddAspNetCoreInstrumentation(aspNetCoreInstrCongfig.Bind);
            }

            // Add more instrumentation as needed
            // https://www.nuget.org/packages?q=opentelemetry.instrumentation

            var otlpExporterConfig = openTelemetryConfig.GetSection("Exporters:Otlp");
            var enableOtlpExporter = tracingConfig.GetValue("Exporters:Otlp", defaultValue: false);
            if (enableOtlpExporter && otlpExporterConfig.Exists())
            {
                builder.AddOtlpExporter(otlpExporterConfig.Bind);
            }

            // Add more exporters as needed 
            // https://www.nuget.org/packages?q=opentelemetry.exporter
        });
    }

    private static IOpenTelemetryBuilder WithCustomMetrics(this IOpenTelemetryBuilder builder, IConfigurationSection openTelemetryConfig)
    {
        var metricsConfig = openTelemetryConfig.GetSection("Metrics");
        if (!metricsConfig.Exists())
            return builder;

        return builder.WithMetrics(builder =>
        {
            var meters = metricsConfig.GetSection("Meters").Get<string[]>();
            if (meters is not null)
            {
                builder.AddMeter(meters);
            }

            var enableAspNetCoreInstr = metricsConfig.GetValue("Instrumentation:AspNetCore", defaultValue: false);
            if (enableAspNetCoreInstr)
            {
                builder.AddAspNetCoreInstrumentation();
            }

            // Add more instrumentation as needed
            // https://www.nuget.org/packages?q=opentelemetry.instrumentation

            var otlpExporterConfig = openTelemetryConfig.GetSection("Exporters:Otlp");
            var enableOtlpExporter = metricsConfig.GetValue("Exporters:Otlp", defaultValue: false);
            if (enableOtlpExporter && otlpExporterConfig.Exists())
            {
                builder.AddOtlpExporter(otlpExporterConfig.Bind);
            }

            // Add more exporters as needed 
            // https://www.nuget.org/packages?q=opentelemetry.exporter
        });
    }

    private sealed record class ServiceOptions
    {
        public required string Name { get; init; }
        public string? Namespace { get; init; }
        public string? Version { get; init; }
        public bool AutoGenerateInstanceId { get; init; }
        public string? InstanceId { get; init; }
    }
}

