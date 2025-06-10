namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices;

public class TelemetryOptions
{
    public TelemetryOptions()
    {
        Logger = new(this);
        ActivitySource = new(this);
        Meter = new(this);
    }

    public string Name
    {
        get;
        set => field = value ?? throw new ArgumentNullException(nameof(value));
    } = string.Empty;

    public string? Version { get; set; }
    public IEnumerable<KeyValuePair<string, object?>>? Tags { get; set; }

    public TelemetryElementOptions Logger { get; }
    public TelemetryElementOptions ActivitySource { get; }
    public TelemetryElementOptions Meter { get; }
}

public class TelemetryElementOptions
{
    private readonly TelemetryOptions _parent;

    internal TelemetryElementOptions(TelemetryOptions parent)
    {
        _parent = parent;
    }

    public string Name => _parent.Name;

    private bool _versionSet = false;
    public string? Version
    {
        get => _versionSet ? field : _parent.Version;
        set
        {
            field = value;
            _versionSet = true;
        }
    }

    private bool _tagsSet = false;
    public IEnumerable<KeyValuePair<string, object?>>? Tags
    {
        get => _tagsSet ? field : _parent.Tags;
        set
        {
            field = value;
            _tagsSet = true;
        }
    }
}

public class TelemetryOptions<TService> : TelemetryOptions
    where TService : ITelemetry
{
}
