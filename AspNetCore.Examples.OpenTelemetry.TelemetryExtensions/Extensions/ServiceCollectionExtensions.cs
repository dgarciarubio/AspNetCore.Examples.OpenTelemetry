using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryExtensions.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelemetryService<TService, TImplementation>(this IServiceCollection services)
        where TService : class, ITelemetry
        where TImplementation : Telemetry, TService
    {
        services.TryAddSingleton<TService, TImplementation>();
        if (typeof(TImplementation).IsGenericTelemetryDerived(out var categoryNameType))
        {
            services.TryAddKeyedSingleton(categoryNameType.GetCategoryName(), (sp, _) => sp.GetRequiredService<TService>());
        }
        foreach (var @interface in typeof(TImplementation).GetImplementedGenericTelemetryInterfaces())
        {
            services.TryAddSingleton(@interface.InterfaceType, sp => sp.GetRequiredService<TService>());
        }
    }
}

internal static class TypeExtensions
{
    public static string GetCategoryName(this Type categoryNameType)
    {
        return CategoryNameHelper.GetFor(categoryNameType);
    }

    public static bool IsGenericTelemetryDerived(this Type type, [MaybeNullWhen(returnValue: false)] out Type categoryNameType)
    {
        categoryNameType = null;
        Type? currentType = type;
        while (currentType != null)
        {
            if (currentType.IsGenericTelemetryClass(out categoryNameType))
            {
                return true;
            }
            currentType = currentType.BaseType;
        }
        return false;
    }

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

    private static bool IsGenericTelemetryClass(this Type type, [MaybeNullWhen(returnValue: false)] out Type categoryNameType)
    {
        categoryNameType = null;
        if (type.IsClass &&
            type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(Telemetry<>))
        {
            categoryNameType = type.GenericTypeArguments.Single();
            return true;
        }
        return false;
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
