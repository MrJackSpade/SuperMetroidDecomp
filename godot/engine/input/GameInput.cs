using System.Collections.Immutable;

namespace Engine.Input;

/// <summary>
/// What the player is asking for in one update: the actions held, and the actions newly pressed since the update
/// before. Made once per update by the <see cref="InputSampler"/> and the same for everything that reads it.
/// </summary>
public sealed class GameInput
{
    /// <summary>The actions held during this update.</summary>
    private readonly ImmutableHashSet<GameAction> _held;

    /// <summary>The actions pressed since the update before.</summary>
    private readonly ImmutableHashSet<GameAction> _pressed;

    /// <summary>Input with the given actions held and newly pressed.</summary>
    public GameInput(IEnumerable<GameAction> held, IEnumerable<GameAction> pressed)
    {
        _held = [.. held];
        _pressed = [.. pressed];
    }

    /// <summary>Nothing held and nothing pressed.</summary>
    public static GameInput None { get; } = new([], []);

    /// <summary>Whether the action is held during this update.</summary>
    public bool IsHeld(GameAction action) => _held.Contains(action);

    /// <summary>Whether the action was pressed since the update before, even if it has been let go again.</summary>
    public bool WasPressed(GameAction action) => _pressed.Contains(action);
}
