using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices;

internal static class TelemetryNameHelper
{
    private static readonly ConcurrentDictionary<Type, string> Names = new();

    public static string GetName<TTelemetryName>()
    {
        return Names.GetOrAdd(typeof(TTelemetryName), (type) =>
        {
            var observer = new LoggerFactoryCategoryNameObserver();
            _ = observer.CreateLogger<TTelemetryName>();
            return observer.CategoryName!;
        });
    }

    public static string GetName(Type telemetryNameType)
    {
        return Names.GetOrAdd(telemetryNameType, (type) =>
        {
            var observer = new LoggerFactoryCategoryNameObserver();
            _ = observer.CreateLogger(telemetryNameType);
            return observer.CategoryName!;
        });
    }

    private class LoggerFactoryCategoryNameObserver : ILoggerFactory
    {
        public string? CategoryName { get; private set; }

        public ILogger CreateLogger(string categoryName)
        {
            CategoryName = categoryName;
            return NullLogger.Instance;
        }

        public void AddProvider(ILoggerProvider provider) { }

        public void Dispose() { }
    }
}
