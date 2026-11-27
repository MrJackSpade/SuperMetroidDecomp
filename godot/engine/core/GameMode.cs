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

    /// <summary>Whether this mode has taken control at some point, and so is being returned to rather than started.</summary>
    internal bool HasTakenControl => _session is not null;

    /// <summary>
    /// The mode this one is laid over and will hand back to, such as play beneath a pause screen. That mode is kept, and
    /// stays on screen, while this one is in control. Null for a mode that replaces the one before it.
    /// </summary>
    public virtual GameMode? Keeps => null;

    /// <summary>
    /// Whether the music waiting to play gets its turn as this mode's next update opens. It does, unless the mode
    /// says otherwise for that update.
    /// </summary>
    public virtual bool GivesMusicItsTurn => true;

    /// <summary>
    /// What of the sounds' own work closes the update this mode has just played: one turn of the sounds, unless the
    /// mode says otherwise for that update. Asked once the mode has played it.
    /// </summary>
    public virtual ClosingSoundWork SoundWorkAtClose => ClosingSoundWork.Usual;

    /// <summary>
    /// Whether an update this mode has played counts on the session's clock. It does, unless the mode says
    /// otherwise: a mode that holds play still for a message does not.
    /// </summary>
    public virtual bool CountsOnClock => true;

    /// <summary>
    /// Takes control back from a mode that was laid over this one, within the update that ends that mode: whatever
    /// of this mode's own update was left undone when it handed over is done now, and the mode in control next is
    /// returned. Itself, with nothing to do, unless the mode says otherwise.
    /// </summary>
    /// <param name="input">The input of the update control comes back on.</param>
    public virtual GameMode TakeBack(GameInput input) => this;

    /// <summary>
    /// Plays what of this mode goes on beneath a mode laid over it, on an update that mode says so. Nothing, unless
    /// the mode says otherwise.
    /// </summary>
    public virtual void StepBeneath()
    {
    }

    /// <summary>
    /// Called on every update a message laid over this mode plays, before the message's own part of it: what of this
    /// mode goes on whatever covers it is played here, as a fade of the whole screen goes on. Nothing, unless the mode
    /// says otherwise.
    /// </summary>
    public virtual void StepCovered()
    {
    }

    /// <summary>Called once as this mode takes control, before its first update.</summary>
    protected virtual void Enter()
    {
    }

    /// <summary>Called as control comes back to this mode from one that was laid over it.</summary>
    protected internal virtual void Resume()
    {
    }

    /// <summary>
    /// Runs one update of this mode and returns the mode in control for the next update: itself to carry on, or a new
    /// mode to hand over to.
    /// </summary>
    public abstract GameMode Step(GameInput input);
}
