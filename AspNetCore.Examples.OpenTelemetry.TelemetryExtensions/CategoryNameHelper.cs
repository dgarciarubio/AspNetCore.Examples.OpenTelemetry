using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics;

internal static class CategoryNameHelper
{
    private static readonly ConcurrentDictionary<Type, string> _cache = new();

    public static string GetFor<TCategoryName>()
    {
        return _cache.GetOrAdd(typeof(TCategoryName), _ => GetForInternal<TCategoryName>());
    }

    public static string GetFor(Type categoryNameType)
    {
        return _cache.GetOrAdd(categoryNameType, _ =>
        {
            var genericGet = typeof(CategoryNameHelper)
                .GetMethod(nameof(GetForInternal))!
                .MakeGenericMethod(categoryNameType);
            return (string)genericGet
                .Invoke(obj: null, parameters: [])!;
        });
    }

    private static string GetForInternal<TCategoryName>()
    {
        var observer = new LoggerFactoryCategoryNameObserver();
        observer.CreateLogger<TCategoryName>();
        return observer.CategoryName;
    }
}

