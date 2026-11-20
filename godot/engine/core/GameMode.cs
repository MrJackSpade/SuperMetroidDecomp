using Engine.Input;
using Godot;

namespace Engine.Core;

/// <summary>
/// One top-level state of a game, with its own screen and its own update: a title screen, play itself, a pause screen.
/// Exactly one mode is in control at a time. Each decides for itself when it is done and names the mode that takes over.
/// </summary>
public abstract partial class GameMode : Node
{
    /// <summary>The session this mode belongs to, once it has taken control.</summary>
    private GameSession? _session;

    /// <summary>The session this mode belongs to.</summary>
    /// <exception cref="InvalidOperationException">The mode has not taken control yet.</exception>
    protected GameSession Session =>
        _session ?? throw new InvalidOperationException($"{GetType().Name} has not taken control of a session yet.");

    /// <summary>Gives this mode control. The session calls this once, before the mode's first update.</summary>
    internal void TakeControl(GameSession session)
    {
        if (_session is not null)
            throw new InvalidOperationException($"{GetType().Name} has already taken control of a session.");
        _session = session;
        Enter();
    }

    /// <summary>Called once as this mode takes control, before its first update.</summary>
    protected virtual void Enter()
    {
    }

    /// <summary>
    /// Runs one update of this mode and returns the mode in control for the next update: itself to carry on, or a new
    /// mode to hand over to.
    /// </summary>
    public abstract GameMode Step(GameInput input);
}
