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
/// Lifetime: the occupant's. A new clock is stopped: a place gets one when a new occupant is bound, whose spawn starts it,
/// or when its occupant is killed, and keeps a deleted occupant's clock with the rest of what that occupant left. A frame
/// shown for good, an animation resting at its end and an animation's end stop the clock (#2454, #2532): the frame shown
/// stays, and the animation takes no step until its owner starts or steps it again, as an owner that chooses another
/// picture does. An owner whose animation must take no step stops the clock itself (<see cref="Stop"/>); one whose
/// animation must wait a while where it stands, its hold kept, suspends it (<see cref="Suspend"/>).
/// <para>
/// The hold is a plain count of updates (#2532). A running clock always holds one update or more: every start, hold,
/// shown frame and pause lasts at least an update, a lengthening adds updates, and the update the hold runs out on is the
/// one its owner shows the animation's next frame, pauses, holds the frame or stops the clock on.
/// </para>
/// </remarks>
public sealed class EnemyAnimationClock
{
    /// <summary>
    /// Updates left before the animation takes its next step: one or more while the clock runs, and none from the update
    /// the hold runs out until its owner shows, pauses or holds the frame again. A stopped or suspended clock keeps the
    /// count it had, which a lengthening still adds to.
    /// </summary>
    public int Hold { get; private set; }

    /// <summary>
    /// Whether the animation showed a new frame on this update: set when a step shows one, cleared on each update its
    /// frame holds. A boss body shows its part's frame on the update the part shows a new one.
    /// </summary>
    public bool ShowsNewFrame { get; internal set; }

    /// <summary>
    /// Whether the clock is stopped (#2213): its animation takes no step, and shows no new frame, until its owner starts or
    /// steps it again. A new clock is stopped. Lengthening a stopped hold leaves it stopped.
    /// </summary>
    public bool IsStopped { get; private set; } = true;

    /// <summary>
    /// Whether the clock is suspended (#2463): its hold counts nothing, and its animation takes no step and shows no new
    /// frame, until its owner resumes it, when the hold runs on from where it stood. Suspending is apart from stopping and
    /// from the hold: starting an animation, or stopping or resuming its clock, leaves the other as it is.
    /// </summary>
    public bool IsSuspended { get; private set; }

    /// <summary>Suspends the clock (#2463): the frame shown stays and the hold waits, without counting, until <see cref="Resume"/>.</summary>
    internal void Suspend() => IsSuspended = true;

    /// <summary>Resumes a suspended clock (#2463): its hold counts on from where it stood when the clock was suspended.</summary>
    internal void Resume() => IsSuspended = false;

    /// <summary>Starts the clock for a newly started animation: its first step plays on the next update.</summary>
    internal void Restart() => Run(1);

    /// <summary>
    /// Starts the clock for a newly started animation whose first step plays after <paramref name="updates"/> updates, one
    /// or more.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is below one.</exception>
    internal void RestartAfter(int updates) => Run(updates);

    /// <summary>
    /// Brings the animation's next step to the next update: the playback goes on from where it stands, or from where its
    /// owner has just placed it.
    /// </summary>
    internal void StepOnNextUpdate() => Run(1);

    /// <summary>
    /// Holds the frame shown for <paramref name="updates"/> more updates, one or more: the animation takes its next step on
    /// the last of them. A frame held for good stops the clock instead (<see cref="Stop"/>).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is below one.</exception>
    internal void HoldFor(int updates) => Run(updates);

    /// <summary>
    /// Stops the clock (#2213): the frame shown stays and the animation waits, without counting, until its owner starts or
    /// steps it again.
    /// </summary>
    internal void Stop() => IsStopped = true;

    /// <summary>Runs the clock again with <paramref name="hold"/> updates, one or more, left before the animation's next step.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="hold"/> is below one.</exception>
    private void Run(int hold)
    {
        Hold = Ensure.BetweenInclusive(1, int.MaxValue, hold);
        IsStopped = false;
    }

    /// <summary>
    /// Lengthens the hold by <paramref name="updates"/>, one or more: the animation takes its next step that many updates
    /// later than it would have. A stopped clock stays stopped with the longer hold.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is below one.</exception>
    internal void Extend(int updates) => Hold = checked(Hold + Ensure.BetweenInclusive(1, int.MaxValue, updates));

    /// <summary>
    /// Counts one update off the hold and reports whether the animation takes its next step now, clearing
    /// <see cref="ShowsNewFrame"/> while the frame holds.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The clock runs with no hold left: its owner neither showed, paused nor held a frame, nor stopped the clock, on the
    /// update its last hold ran out.
    /// </exception>
    internal bool Expire()
    {
        if (IsStopped || IsSuspended)
        {
            ShowsNewFrame = false;
            return false;
        }
        if (Hold == 0)
        {
            throw new InvalidOperationException(
                "An enemy animation clock runs with no hold left: its owner did not show, pause or hold a frame, or stop the clock, when its last hold ran out.");
        }
        if (--Hold == 0)
            return true;
        ShowsNewFrame = false;
        return false;
    }

    /// <summary>The animation's step showed a new frame for <paramref name="duration"/> updates, one or more.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="duration"/> is below one.</exception>
    internal void Show(int duration)
    {
        Run(duration);
        ShowsNewFrame = true;
    }

    /// <summary>
    /// The animation's step showed a new frame for good (#2454): the clock stops, so the frame stays and the animation takes
    /// no step until its owner starts or steps it again.
    /// </summary>
    internal void ShowForGood()
    {
        Stop();
        ShowsNewFrame = true;
    }

    /// <summary>The animation's step paused on its frame for <paramref name="duration"/> updates, one or more.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="duration"/> is below one.</exception>
    internal void Pause(int duration) => Run(duration);
}
