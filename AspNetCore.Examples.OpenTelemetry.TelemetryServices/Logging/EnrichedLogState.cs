using System.Collections;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices.Logging;

internal readonly struct EnrichedLogState<T> : IReadOnlyList<KeyValuePair<string, object?>>
{
    private readonly IReadOnlyList<KeyValuePair<string, object?>> _enrichState;
    private readonly IReadOnlyList<KeyValuePair<string, object?>> _originalState;

    public EnrichedLogState(IReadOnlyList<KeyValuePair<string, object?>> enrichState, T originalState)
    {
        _enrichState = enrichState;
        _originalState =
            originalState as IReadOnlyList<KeyValuePair<string, object?>> ??
            (originalState as IEnumerable<KeyValuePair<string, object?>>)?.ToArray() ??
            [new(nameof(OriginalState), originalState)];
        OriginalState = originalState;
    }

    public T OriginalState { get; }

    public int Count => _enrichState.Count + _originalState.Count;

    public KeyValuePair<string, object?> this[int index] => index < _enrichState.Count ? _enrichState[index] : _originalState[index - _enrichState.Count];

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _enrichState.Concat(_originalState).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}