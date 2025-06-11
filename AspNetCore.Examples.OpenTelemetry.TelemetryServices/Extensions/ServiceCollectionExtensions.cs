using AspNetCore.Examples.OpenTelemetry.TelemetryServices;
using AspNetCore.Examples.OpenTelemetry.TelemetryServices.Extensions;
using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services, Action<TelemetryBuilder>? configure = null)
    {
        var builder = services.AddTelemetry();
        configure?.Invoke(builder);
        return services;
    }

    public static TelemetryServiceBuilder AddTelemetryFor(this IServiceCollection services, string name, Action<TelemetryOptions<ITelemetry>>? configureOptions = null)
        => services.AddTelemetry().AddFor(name, configureOptions);

    public static TelemetryServiceBuilder AddTelemetryFor<TTelemetryName>(this IServiceCollection services, Action<TelemetryOptions<ITelemetry<TTelemetryName>>>? configureOptions = null)
        => services.AddTelemetry().AddFor(configureOptions);

    public static TelemetryServiceBuilder AddTelemetry<TService>(this IServiceCollection services, Action<TelemetryOptions<TService>>? configureOptions = null)
        where TService : Telemetry
        => services.AddTelemetry().Add(configureOptions);

    public static TelemetryServiceBuilder AddTelemetry<TService, TImplementation>(this IServiceCollection services, Action<TelemetryOptions<TImplementation>>? configureOptions = null)
        where TService : class, ITelemetry
        where TImplementation : Telemetry, TService
        => services.AddTelemetry().Add<TService, TImplementation>(configureOptions);

    public static TelemetryServiceBuilder AddTelemetry<TService>(this IServiceCollection services, string name, Action<TelemetryOptions<TService>>? configureOptions = null)
        where TService : Telemetry
        => services.AddTelemetry().Add(name, configureOptions);

    public static TelemetryServiceBuilder AddTelemetry<TService, TImplementation>(this IServiceCollection services, string name, Action<TelemetryOptions<TImplementation>>? configureOptions = null)
        where TService : class, ITelemetry
        where TImplementation : Telemetry, TService
        => services.AddTelemetry().Add<TService, TImplementation>(name, configureOptions);

    private static TelemetryBuilder AddTelemetry(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.AddLogging();
        services.AddMetrics();
        services.TryAddSingleton(typeof(ITelemetry<>), typeof(Telemetry<>));

        return new TelemetryBuilder(services);
    }

}



