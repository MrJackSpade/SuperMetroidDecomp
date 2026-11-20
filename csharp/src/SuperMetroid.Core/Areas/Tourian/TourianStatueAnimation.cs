namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The Tourian statue room's shared animation state (#1549), which it alone changes through named steps (#2545): which
/// boss statues are still animating, where the releases stand (<see cref="TourianStatueReleasePhase"/>) and whether the
/// entrance statue has finished sinking. The statue animations' cues, the descent and the entrance statue's base
/// decoration take the steps; how far the statue has sunk is the descent's own (<see cref="ITourianStatueRoom.StatueOffset"/>,
/// #2033). The statue room holds it (<see cref="ITourianStatueRoom.Animation"/>, #2486).
/// </summary>
/// <remarks>
/// The sinking's end is a latch of its own beside the releases: the base decoration marks it once the statues rest, and a
/// room entered after the descent has unlocked Tourian marks it on every update while its releases never begin.
/// </remarks>
public sealed class TourianStatueAnimation
{
    /// <summary>
    /// Whether the entrance statue has finished sinking. The statue's base decoration sets it once the sinking animation
    /// has ended.
    /// </summary>
    public bool Finished { get; private set; }

    /// <summary>The statues running their idle or release animation.</summary>
    public TourianGreyStatues Animating { get; private set; }

    /// <summary>Where the releases stand: none, one playing, or all four released with or without the last still playing.</summary>
    public TourianStatueReleasePhase Release { get; private set; }

    /// <summary>
    /// Whether one statue is playing its release; the others keep idling until it finishes, so releases play one at a
    /// time.
    /// </summary>
    public bool ReleaseInProgress =>
        Release is TourianStatueReleasePhase.OneReleasing or TourianStatueReleasePhase.AllReleasedWhileLastReleases;

    /// <summary>Whether the descent has latched that all four grey-statue events exist, before it waits for the last release.</summary>
    public bool AllReleased =>
        Release is TourianStatueReleasePhase.AllReleasedWhileLastReleases or TourianStatueReleasePhase.AllReleased;

    /// <summary>
    /// Whether no statue is animating, none is releasing and the descent has latched nothing: the room-load state, in
    /// which the entrance statue's decorations are finished.
    /// </summary>
    public bool AtRest => Animating == TourianGreyStatues.None && Release == TourianStatueReleasePhase.NoneReleasing;

    /// <summary>The entrance statue has finished sinking.</summary>
    internal void FinishSinking() => Finished = true;

    /// <summary><paramref name="statue"/> starts running its idle or release animation.</summary>
    internal void MarkAnimating(TourianGreyStatues statue) => Animating |= statue;

    /// <summary><paramref name="statue"/> stops running its animation.</summary>
    internal void StopAnimating(TourianGreyStatues statue) => Animating &= ~statue;

    /// <summary>A statue claims the room's one release, which the others wait their turn behind.</summary>
    internal void ClaimRelease() => Release = AllReleased
        ? TourianStatueReleasePhase.AllReleasedWhileLastReleases
        : TourianStatueReleasePhase.OneReleasing;

    /// <summary><paramref name="statue"/> finishes its release and stops animating.</summary>
    internal void FinishRelease(TourianGreyStatues statue)
    {
        Release = AllReleased ? TourianStatueReleasePhase.AllReleased : TourianStatueReleasePhase.NoneReleasing;
        StopAnimating(statue);
    }

    /// <summary>The descent latches that all four grey-statue events exist, whether or not the last release still plays.</summary>
    internal void LatchAllReleased() => Release = ReleaseInProgress
        ? TourianStatueReleasePhase.AllReleasedWhileLastReleases
        : TourianStatueReleasePhase.AllReleased;
}
