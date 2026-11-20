using Engine.Input;

namespace Engine.Tests.Input;

/// <summary>An input source for checks that decide what is down as they go: each update gives whatever is down at the moment.</summary>
public sealed class SteeredInputSource : IInputSource
{
    /// <summary>The actions down at the moment.</summary>
    public HashSet<GameAction> Down { get; } = [];

    /// <inheritdoc/>
    public IReadOnlySet<GameAction> TakeActionsDown() => new HashSet<GameAction>(Down);
}
