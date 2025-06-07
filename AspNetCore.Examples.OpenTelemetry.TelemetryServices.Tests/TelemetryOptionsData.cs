using AspNetCore.Examples.OpenTelemetry.TelemetryServices;
using AspNetCore.Examples.OpenTelemetry.TelemetryServices.Tests;
using Xunit.Sdk;

[assembly: RegisterXunitSerializer(typeof(JsonDataSerializer), typeof(TelemetryOptions))]

namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices.Tests;

public class TelemetryOptionsData : TheoryData<TelemetryOptions>
{
    public TelemetryOptionsData()
    {
        Add(new TelemetryOptions
        {
            Name = "Name",
        });
        Add(new TelemetryOptions
        {
            Name = "Name",
            Version = "V1.0",
            Tags = new() { { "TagName", "TagValue" } },
        });
    }
}

public class NamedTelemetryOptionsData : TheoryData<TelemetryOptions>
{
    public NamedTelemetryOptionsData(string name)
    {
        Add(new TelemetryOptions
        {
            Name = name,
        });
        Add(new TelemetryOptions
        {
            Name = name,
            Version = "V1.0",
            Tags = new() { { "TagName", "TagValue" } },
        });
    }
}


