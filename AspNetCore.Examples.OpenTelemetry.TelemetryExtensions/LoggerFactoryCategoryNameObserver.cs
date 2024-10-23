using Microsoft.Extensions.Logging;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics;

internal class LoggerFactoryCategoryNameObserver(ILoggerFactory? inner) : ILoggerFactory
{
    public string? CategoryName { get; private set; }

    public ILogger CreateLogger(string categoryName)
    {
        CategoryName = categoryName;
        return inner?.CreateLogger(categoryName)!; //Value should never be used if inner factory is null
    }

    public void AddProvider(ILoggerProvider provider) => inner?.AddProvider(provider);

    public void Dispose() { }
}

