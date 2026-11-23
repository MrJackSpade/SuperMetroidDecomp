namespace Engine.Input;

/// <summary>
/// Makes each update's <see cref="GameInput"/>. It asks its source once per update, and from what was down in the
/// update before works out what is newly pressed. Nothing else reads the player's devices.
/// </summary>
public sealed class InputSampler
{
    /// <summary>Where the actions come from.</summary>
    private readonly IInputSource _source;

    /// <summary>The actions that were down in the update before.</summary>
    private IReadOnlySet<GameAction> _downLastUpdate = new HashSet<GameAction>();

    /// <summary>A sampler reading <paramref name="source"/>.</summary>
    public InputSampler(IInputSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        _source = source;
    }

    /// <summary>The input for this update. Called once per update, by the game loop.</summary>
    public GameInput Sample()
    {
        IReadOnlySet<GameAction> down = _source.TakeActionsDown();
        var pressed = down.Where(action => !_downLastUpdate.Contains(action)).ToList();
        _downLastUpdate = new HashSet<GameAction>(down);
        return new GameInput(down, pressed);
    }
}
