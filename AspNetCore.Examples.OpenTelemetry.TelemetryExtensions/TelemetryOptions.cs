#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public class TelemetryOptions
{
    public string? Version { get; init; }
    public IEnumerable<KeyValuePair<string, object?>>? Tags { get; init; }
    public object? Scope { get; init; }
}
