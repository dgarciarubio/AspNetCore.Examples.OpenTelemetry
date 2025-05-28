using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public class TelemetryOptions
{
    public TelemetryOptions(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        Name = name;
    }

    public string Name { get; }
    public string? Version { get; init; }
    public IEnumerable<KeyValuePair<string, object?>>? Tags { get; init; }
    public object? Scope { get; init; }
}

public class TelemetryOptions<TTelemetryName> : TelemetryOptions
{
    public static readonly new string Name = CategoryNameHelper.GetTelemetryName();

    public TelemetryOptions()
        : base(Name)
    {
    }

    private static class CategoryNameHelper
    {
        public static string GetTelemetryName()
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
}

