using Engine.Input;

namespace Engine.Tests.Input;

/// <summary>An input source for checks: each update takes the next set of actions it was given, then nothing.</summary>
public sealed class ScriptedInputSource : IInputSource
{
    /// <summary>The actions down in each update still to come.</summary>
    private readonly Queue<GameAction[]> _updates;

    /// <summary>A source giving <paramref name="updates"/>, one set of actions per update, in order.</summary>
    public ScriptedInputSource(params GameAction[][] updates) => _updates = new Queue<GameAction[]>(updates);

    /// <inheritdoc/>
    public IReadOnlySet<GameAction> TakeActionsDown() =>
        new HashSet<GameAction>(_updates.Count > 0 ? _updates.Dequeue() : []);
}
