namespace Engine.Input;

/// <summary>
/// A recording being played: each update it gives the actions the recording holds for that update, and which of
/// them are newly pressed.
/// </summary>
public sealed class RecordedInputSource : IInputSource
{
    private readonly DemoRecording _recording;
    private IReadOnlyList<GameAction> _heldBefore = [];
    private int _next;

    /// <summary>A source playing <paramref name="recording"/> from its first update.</summary>
    public RecordedInputSource(DemoRecording recording)
    {
        ArgumentNullException.ThrowIfNull(recording);
        _recording = recording;
    }

    /// <summary>Whether the recording has been played to its end. One that goes on holding its last actions never is.</summary>
    public bool IsOver => _next >= _recording.Updates.Count && !_recording.HoldsLast;

    /// <inheritdoc/>
    public IReadOnlySet<GameAction> TakeActionsDown() => new HashSet<GameAction>(Take().Held);

    /// <summary>
    /// The input of the recording's next update: what it holds, and what it says is newly pressed, or, where it does
    /// not say, whatever it holds that it did not hold the update before.
    /// </summary>
    public GameInput Next()
    {
        (IReadOnlyList<GameAction> held, IReadOnlyList<GameAction>? pressed) = Take();
        IReadOnlyList<GameAction> before = _heldBefore;
        _heldBefore = held;
        return new GameInput(held, pressed ?? held.Where(action => !before.Contains(action)).ToList());
    }

    /// <summary>Takes the recording's next update: nothing once it is over, its last actions for one that holds them.</summary>
    private (IReadOnlyList<GameAction> Held, IReadOnlyList<GameAction>? Pressed) Take()
    {
        if (_next < _recording.Updates.Count)
        {
            int update = _next++;
            return (_recording.Updates[update], _recording.Pressed[update]);
        }

        // Held on past the end, nothing is pressed again.
        return _recording.HoldsLast ? (_recording.Updates[^1], []) : ([], null);
    }
}
