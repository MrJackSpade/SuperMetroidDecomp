namespace SuperMetroid.Core.Animation;

/// <summary>
/// A family of animations that never pause (#2553), whether or not it may hide: what a projectile's timeline plays, each
/// play coming to an outcome that is no pause, or to a hidden stretch in a family that hides.
/// </summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
internal interface IUnpausedAnimationPlay<TSegment, TFrame, TEvent>
    where TSegment : struct
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>
    /// Plays <paramref name="animation"/> on from <paramref name="place"/> to its next frame, as
    /// <see cref="AnimationSegmentPlay{TSegment, TFrame, TEvent, TBeat, TShown}"/> plays its segments, each event going to
    /// <paramref name="react"/>, which may continue the play elsewhere.
    /// </summary>
    IUnpausedHidingAnimationShown<TSegment, TFrame> PlayToNextFrame(
        ref Animation<TSegment> animation, ref AnimationPlace place, Func<TEvent, AnimationPlayback<TSegment>, AnimationContinuation<TSegment>?> react);
}
