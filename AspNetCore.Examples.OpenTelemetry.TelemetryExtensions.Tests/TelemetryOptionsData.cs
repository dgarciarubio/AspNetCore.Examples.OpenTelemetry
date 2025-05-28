using System.Diagnostics;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryExtensions.Tests;

public class TelemetryOptionsData : TheoryData<TelemetryOptions>
{
    public TelemetryOptionsData()
    {
        Add(new TelemetryOptions("Name"));
        Add(new TelemetryOptions("Name")
        {
            Version = "V1.0",
            Tags = new Dictionary<string, object?> { { "TagName", "TagValue" } },
            Scope = "Scope",
        });
    }
}

public class GenericTelemetryOptionsData : TheoryData<TelemetryOptions<TelemetryName>?>
{
    public GenericTelemetryOptionsData()
    {
        Add(null);
        Add(new TelemetryOptions<TelemetryName>
        {
            Version = "V1.0",
            Tags = new Dictionary<string, object?> { { "TagName", "TagValue" } },
            Scope = "Scope",
        });
    }
}