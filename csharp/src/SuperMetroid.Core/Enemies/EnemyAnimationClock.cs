using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy occupant's animation clock (#1624): how long the frame its animation shows holds before the animation takes
/// its next step, and whether this update showed a new frame. The occupant's species state holds what plays and where it
/// stands (its <see cref="AnimationPlayback{TSegment}"/>, which counts its own repeated phases' plays, #2006); this clock
/// times that playback, and starting an animation restarts it (<see cref="Restart"/>). The occupant's behaviour keeps its
/// own timers in its species state; nothing but the animation counts on this clock.
/// </summary>
/// <remarks>
/// Lifetime: the occupant's. A place gets a stopped clock when a new occupant is bound or its occupant is killed, and
/// keeps a deleted occupant's clock with the rest of what that occupant left. The hold wraps from zero as every
/// countdown of the game does, so a held animation, whose clock nothing restarts, never steps again.
/// </remarks>
public sealed class EnemyAnimationClock
{
    /// <summary>Updates left before the animation takes its next step.</summary>
    public WrappingCountdown Hold { get; private set; }

    /// <summary>
    /// Whether the animation showed a new frame on this update: set when a step shows one, cleared on each update its
    /// frame holds. A boss body shows its part's frame on the update the part shows a new one.
    /// </summary>
    public bool ShowsNewFrame { get; internal set; }

    /// <summary>
    /// Whether the clock is stopped (#2213): its animation takes no step, and shows no new frame, until its owner starts or
    /// steps it again. Lengthening a stopped hold leaves it stopped.
    /// </summary>
    public bool IsStopped { get; private set; }

    /// <summary>Starts the clock for a newly started animation: its first step plays on the next update.</summary>
    internal void Restart() => Run(WrappingCountdown.Start(1));

    /// <summary>
    /// Starts the clock for a newly started animation whose first step plays after <paramref name="updates"/> updates,
    /// 0 through 65535.
    /// </summary>
    internal void RestartAfter(int updates) => Run(WrappingCountdown.Start(updates));

    /// <summary>
    /// Brings the animation's next step to the next update: the playback goes on from where it stands, or from where its
    /// owner has just placed it.
    /// </summary>
    internal void StepOnNextUpdate() => Run(WrappingCountdown.Start(1));

    /// <summary>Holds the frame shown for <paramref name="updates"/> more updates, 0 through 65535; zero wraps on the next.</summary>
    internal void HoldFor(int updates) => Run(WrappingCountdown.Start(updates));

    /// <summary>
    /// Stops the clock (#2213): the frame shown stays and the animation waits, without counting, until its owner starts or
    /// steps it again.
    /// </summary>
    internal void Stop() => IsStopped = true;

    /// <summary>Runs the clock again with <paramref name="hold"/> left before the animation's next step.</summary>
    private void Run(WrappingCountdown hold)
    {
        Hold = hold;
        IsStopped = false;
    }

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
        if (IsStopped)
        {
            ShowsNewFrame = false;
            return false;
        }
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
        Run(WrappingCountdown.Start(duration));
        ShowsNewFrame = true;
    }

    /// <summary>
    /// The animation's step showed a new frame for good: the hold is left to wrap from zero, so no later step plays
    /// anything.
    /// </summary>
    internal void ShowForGood() => ShowsNewFrame = true;

    /// <summary>The animation's step paused on its frame for <paramref name="duration"/> updates.</summary>
    internal void Pause(int duration) => Run(WrappingCountdown.Start(duration));
}
