using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy occupant's animation clock (#1624): how long the frame its animation shows holds before the animation takes
/// its next step, the count the animation's counted phases keep, and whether this update showed a new frame. The
/// occupant's species state holds what plays and where it stands (its <see cref="AnimationPlayback{TSegment}"/>); this
/// clock times that playback, and starting an animation restarts it (<see cref="Restart"/>). The occupant's behaviour
/// keeps its own timers in its species state; nothing but the animation counts on this clock.
/// </summary>
/// <remarks>
/// Lifetime: the occupant's. A place gets a stopped clock when a new occupant is bound or its occupant is killed, and
/// keeps a deleted occupant's clock with the rest of what that occupant left. The hold wraps from zero as every
/// countdown of the game does, so a held animation, whose clock nothing restarts, never steps again.
/// </remarks>
public sealed class EnemyAnimationClock : IAnimationRepeatCounter
{
    /// <summary>Updates left before the animation takes its next step.</summary>
    public WrappingCountdown Hold { get; private set; }

    /// <summary>
    /// The plays left of the counted phase playing: a counted phase loads it as it starts and each play's end counts it
    /// down. It stays at zero after the phase for whatever else reads it.
    /// </summary>
    public WrappingCountdown Repeats { get; internal set; }

    /// <inheritdoc />
    WrappingCountdown IAnimationRepeatCounter.Repeats
    {
        get => Repeats;
        set => Repeats = value;
    }

    /// <summary>
    /// Whether the animation showed a new frame on this update: set when a step shows one, cleared on each update its
    /// frame holds. A boss body shows its part's frame on the update the part shows a new one.
    /// </summary>
    public bool ShowsNewFrame { get; internal set; }

    /// <summary>Starts the clock for a newly started animation: its first step plays on the next update, with no repeats counted.</summary>
    internal void Restart()
    {
        Hold = WrappingCountdown.Start(1);
        Repeats = WrappingCountdown.Start(0);
    }

    /// <summary>
    /// Starts the clock for a newly started animation whose first step plays after <paramref name="updates"/> updates,
    /// 0 through 65535, with no repeats counted.
    /// </summary>
    internal void RestartAfter(int updates)
    {
        Hold = WrappingCountdown.Start(updates);
        Repeats = WrappingCountdown.Start(0);
    }

    /// <summary>Clears the repeats without touching the hold: the playback the owner has just placed counts none yet.</summary>
    internal void ClearRepeats() => Repeats = WrappingCountdown.Start(0);

    /// <summary>
    /// Brings the animation's next step to the next update without touching its repeats: the playback goes on from where
    /// it stands, or from where its owner has just placed it.
    /// </summary>
    internal void StepOnNextUpdate() => Hold = WrappingCountdown.Start(1);

    /// <summary>Holds the frame shown for <paramref name="updates"/> more updates, 0 through 65535; zero wraps on the next.</summary>
    internal void HoldFor(int updates) => Hold = WrappingCountdown.Start(updates);

    /// <summary>
    /// Lengthens the hold by <paramref name="updates"/> (a negative count shortens it), wrapping as a countdown carried past
    /// its range does.
    /// </summary>
    internal void Extend(int updates) => Hold = Hold.AddFrames(updates);

    /// <summary>
    /// Counts one update off the hold and reports whether the animation takes its next step now, clearing
    /// <see cref="ShowsNewFrame"/> while the frame holds.
    /// </summary>
    internal bool Expire()
    {
        CountdownStep step = Hold.Decrement();
        Hold = step.Remaining;
        if (step.IsZero)
            return true;
        ShowsNewFrame = false;
        return false;
    }

    /// <summary>The animation's step showed a new frame for <paramref name="duration"/> updates.</summary>
    internal void Show(int duration)
    {
        Hold = WrappingCountdown.Start(duration);
        ShowsNewFrame = true;
    }

    /// <summary>
    /// The animation's step showed a new frame for good: the hold is left to wrap from zero, so no later step plays
    /// anything.
    /// </summary>
    internal void ShowForGood() => ShowsNewFrame = true;

    /// <summary>The animation's step paused on its frame for <paramref name="duration"/> updates.</summary>
    internal void Pause(int duration) => Hold = WrappingCountdown.Start(duration);
}
