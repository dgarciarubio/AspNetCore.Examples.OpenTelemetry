using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.Hosting;
#pragma warning restore IDE0130 // Namespace does not match folder structure
public static class Extensions
{
    private static readonly Type TelemetryType = typeof(Telemetry<>);
    private static readonly Type ITelemetryType = typeof(ITelemetry<>);

    public static IServiceCollection AddTelemetry<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddSingleton<TService, TImplementation>();
        services.AddTelemetryInternal(services => services.GetRequiredService<TImplementation>());
        return services;
    }

    public static IServiceCollection AddTelemetry<TService>(this IServiceCollection services)
        where TService : class
    {
        services.AddSingleton<TService>();
        services.AddTelemetryInternal(services => services.GetRequiredService<TService>());
        return services;
    }

    private static IServiceCollection AddTelemetryInternal<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
        where TService : class
    {
        var implementationType = typeof(TService);
        if (implementationType.TryGetBaseTelemetryType(out var baseTelemetryType))
        {
            services.AddSingleton(baseTelemetryType, implementationFactory);
            var categoryName = baseTelemetryType
                .GetField(nameof(Telemetry<object>.CategoryName), BindingFlags.Static | BindingFlags.NonPublic)?
                .GetValue(obj: null) as string;
            if (categoryName is not null)
            {
                services.AddOpenTelemetry()
                    .WithTracing(t => t.AddSource(categoryName))
                    .WithMetrics(m => m.AddMeter(categoryName));
            }
        }
        foreach (var telemetryInterfaceType in implementationType.GetTelemetryInterfaceTypes())
        {
            services.AddSingleton(telemetryInterfaceType, implementationFactory);
        }

        return services;
    }


    private static bool TryGetBaseTelemetryType(this Type implementationType, [MaybeNullWhen(returnValue: false)] out Type baseTelemetryType)
    {
        baseTelemetryType = null;
        Type? type = implementationType;
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == TelemetryType)
            {
                baseTelemetryType = type;
                return true;
            }
            type = type.BaseType;
        }
        return false;
    }

    private static IEnumerable<Type> GetTelemetryInterfaceTypes(this Type implementationType)
    {
        return implementationType.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == ITelemetryType);
    }
}
