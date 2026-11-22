namespace SuperMetroid.Core.Animation;

/// <summary>Where an owner's reaction to an event continues its play (#1600): another animation, or the same, at a place.</summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <param name="Animation">The animation the play continues in.</param>
/// <param name="Place">The place it continues at, with the plays its repeated phase has left there.</param>
internal readonly partial record struct AnimationContinuation<TSegment>(Animation<TSegment> Animation, AnimationPlace Place)
    where TSegment : struct
{
    /// <summary>
    /// Whether the play counts on the plays its repeated phase had left where the event raised it, rather than
    /// <see cref="Place"/>'s own (#2006).
    /// </summary>
    internal bool CountsOn { get; private init; }

    /// <summary>
    /// A continuation at <paramref name="place"/> of <paramref name="animation"/> that counts on the plays the branching
    /// play had left (#2006): a branch into another run of the same length at the end of a play goes on with those plays, as
    /// a lap count carries over from one pace to another.
    /// </summary>
    internal static AnimationContinuation<TSegment> CountingOn(Animation<TSegment> animation, AnimationPlace place) =>
        new(animation, place) { CountsOn = true };
}
