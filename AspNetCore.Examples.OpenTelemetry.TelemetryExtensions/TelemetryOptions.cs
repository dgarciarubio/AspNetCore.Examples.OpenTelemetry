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
    public static readonly new string Name = TelemetryNameHelper.GetTelemetryName<TTelemetryName>();

    public TelemetryOptions()
        : base(Name)
    {
    }
}
