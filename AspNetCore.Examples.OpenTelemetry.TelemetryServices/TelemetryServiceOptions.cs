namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices;

public class TelemetryOptions
{
    public string Name
    {
        get;
        set => field = value ?? throw new ArgumentNullException(nameof(value));
    } = string.Empty;

    public string? Version { get; set; }
    public IEnumerable<KeyValuePair<string, object?>>? Tags { get; set; }
}

public class TelemetryOptions<TTelemetryService> : TelemetryOptions
    where TTelemetryService : ITelemetry
{
}

