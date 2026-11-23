namespace Engine.Input;

/// <summary>A recording as an input source: each update it gives the actions the recording holds for that update.</summary>
public sealed class RecordedInputSource : IInputSource
{
    private readonly IReadOnlyList<IReadOnlyList<GameAction>> _updates;
    private int _next;

    /// <summary>A source playing <paramref name="recording"/> from its first update.</summary>
    public RecordedInputSource(DemoRecording recording)
    {
        ArgumentNullException.ThrowIfNull(recording);
        _updates = recording.Updates;
    }

    /// <summary>Whether the recording has been played to its end.</summary>
    public bool IsOver => _next >= _updates.Count;

    /// <inheritdoc/>
    public IReadOnlySet<GameAction> TakeActionsDown() =>
        new HashSet<GameAction>(IsOver ? [] : _updates[_next++]);
}
