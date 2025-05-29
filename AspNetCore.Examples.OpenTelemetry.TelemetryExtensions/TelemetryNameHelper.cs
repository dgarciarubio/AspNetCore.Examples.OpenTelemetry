using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics;

internal static class TelemetryNameHelper
{
    public static string GetTelemetryName<TTelemetryName>()
    {
        var observer = new LoggerFactoryCategoryNameObserver();
        _ = observer.CreateLogger<TTelemetryName>();
        return observer.CategoryName!;
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
