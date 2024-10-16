namespace System.Diagnostics;

/// <summary>
/// Options for creating an <see cref="ActivitySource"/>.
/// </summary>
public class ActivitySourceOptions
{
    private string _name;

    /// <summary>
    /// The ActivitySource name.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _name = value;
        }
    }

    /// <summary>
    /// The optional ActivitySource version.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// The optional list of key-value pair tags associated with the ActivitySource.
    /// </summary>
    public IEnumerable<KeyValuePair<string, object?>>? Tags { get; set; }

    /// <summary>
    /// Constructs a new instance of <see cref="ActivitySourceOptions"/>.
    /// </summary>
    /// <param name="name">The ActivitySource name.</param>
    public ActivitySourceOptions(string name)
    {
        Name = name;

        Debug.Assert(_name is not null);
    }
}
