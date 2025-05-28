using AspNetCore.Examples.OpenTelemetry.TelemetryExtensions.Tests;
using System.Diagnostics;
using Xunit.Sdk;

[assembly: RegisterXunitSerializer(typeof(JsonDataSerializer), typeof(TelemetryOptions), typeof(TelemetryOptions<>))]

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


public class TelemetryOptionsData<TTelemetryName> : TheoryData<TelemetryOptions<TTelemetryName>?>
{
    public TelemetryOptionsData()
    {
        Add(null!);
        Add(new TelemetryOptions<TTelemetryName>
        {
            Version = "V1.0",
            Tags = new Dictionary<string, object?> { { "TagName", "TagValue" } },
            Scope = "Scope",
        });
    }
}
