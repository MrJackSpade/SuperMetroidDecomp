namespace SuperMetroid.Core.Animation;

/// <summary>
/// The play of animations that raise no event (#2553): <see cref="NoAnimationCue"/> has no member, so playing one takes no
/// reaction to carry an event out.
/// </summary>
internal static class SilentAnimationSegments
{
    /// <summary>
    /// Plays <paramref name="animation"/> on from <paramref name="place"/> to the next frame it shows, as
    /// <see cref="AnimationSegments{TSegment, TFrame, TEvent}.PlayToNextFrame(Animation{TSegment}, ref AnimationPlace, Action{TEvent})"/>
    /// plays an animation with events.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The animation passes every beat without showing a frame, or ends a play of a repeated phase it stands in with no
    /// plays left.
    /// </exception>
    internal static AnimationShown<TSegment, TFrame> PlayToNextFrame<TSegment, TFrame>(
        this AnimationSegments<TSegment, TFrame, NoAnimationCue> segments, Animation<TSegment> animation, ref AnimationPlace place)
        where TSegment : struct
        where TFrame : struct =>
        Ensure.NotNull(segments).PlayToNextFrame(animation, ref place, static (NoAnimationCue _) => { });
}
