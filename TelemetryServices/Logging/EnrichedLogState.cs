using System.Collections;

namespace TelemetryServices.Logging;

internal readonly struct EnrichedLogState<T>(IReadOnlyList<KeyValuePair<string, object?>> enrichState, T originalState)
    : IReadOnlyList<KeyValuePair<string, object?>>
{
    private readonly IReadOnlyList<KeyValuePair<string, object?>> _enrichState = enrichState;
    private readonly IReadOnlyList<KeyValuePair<string, object?>> _originalState = originalState switch
    {
        IReadOnlyList<KeyValuePair<string, object?>> list => list,
        IEnumerable<KeyValuePair<string, object?>> enumerable => [.. enumerable],
        null => [],
        _ => [new("State", originalState)],
    };

    public T OriginalState { get; } = originalState;

    public int Count => _enrichState.Count + _originalState.Count;

    public KeyValuePair<string, object?> this[int index] => index < _enrichState.Count ? _enrichState[index] : _originalState[index - _enrichState.Count];

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _enrichState.Concat(_originalState).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}