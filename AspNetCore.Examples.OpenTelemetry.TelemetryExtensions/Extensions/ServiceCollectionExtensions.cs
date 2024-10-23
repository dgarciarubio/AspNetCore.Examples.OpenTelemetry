using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryExtensions.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelemetryService<TService, TImplementation>(this IServiceCollection services)
        where TService : class, ITelemetry
        where TImplementation : class, TService
    {
        services.TryAddSingleton<TService, TImplementation>();
        foreach (var @interface in typeof(TImplementation).GetImplementedGenericTelemetryInterfaces())
        {
            services.TryAddSingleton(@interface.InterfaceType, sp => sp.GetRequiredService<TService>());
        }
    }
}

internal static class TypeExtensions
{
    public static IEnumerable<(Type InterfaceType, Type CategoryNameType)> GetImplementedGenericTelemetryInterfaces(this Type type)
    {
        foreach (var interfaceType in type.GetInterfaces())
        {
            if (interfaceType.IsGenericTelemetryInterface(out var categoryNameType))
            {
                yield return (interfaceType, categoryNameType);
            }
        }
    }

    private static bool IsGenericTelemetryInterface(this Type type, [MaybeNullWhen(returnValue: false)] out Type categoryNameType)
    {
        categoryNameType = null;
        if (type.IsInterface &&
            type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(ITelemetry<>))
        {
            categoryNameType = type.GenericTypeArguments.Single();
            return true;
        }
        return false;
    }
}
