namespace TelemetryServices.Tests.TheoryData;

public class TelemetryOptionsData : TheoryData<TelemetryOptions>
{
    public TelemetryOptionsData()
        : this(name: null)
    { }

    public TelemetryOptionsData(string? name = null)
    {
        Add(new TelemetryOptions
        {
            Name = name ?? "Name",
        });
        Add(new TelemetryOptions
        {
            Name = name ?? "Name",
            Version = "V1.0",
            Tags = new Dictionary<string, object?>() { ["Tag"] = "TagValue" },
        });
    }
}


