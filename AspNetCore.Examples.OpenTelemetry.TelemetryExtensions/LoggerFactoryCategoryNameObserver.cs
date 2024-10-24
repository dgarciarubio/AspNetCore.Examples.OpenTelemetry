using Microsoft.Extensions.Logging;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics;

internal class LoggerFactoryCategoryNameObserver(ILoggerFactory? inner = null) : ILoggerFactory
{
    private string? _categoryName = null;
    public string CategoryName => _categoryName ?? throw new InvalidOperationException("Could not retrieve category name from generic logger.");

    public ILogger CreateLogger(string categoryName)
    {
        _categoryName = categoryName;
        return inner?.CreateLogger(categoryName)!; //Value should never be used if inner factory is null
    }

    public void AddProvider(ILoggerProvider provider) => inner?.AddProvider(provider);

    public void Dispose() { }
}

