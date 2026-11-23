namespace SuperMetroid.Core.Animation;

/// <summary>
/// The play of animations that raise no event (#2553): <see cref="NoAnimationCue"/> has no member, so playing one takes no
/// reaction to carry an event out.
/// </summary>
internal static partial class SilentAnimationSegments
{
    /// <summary>
    /// Plays <paramref name="animation"/> on from <paramref name="place"/> to the next frame it shows, as
    /// <see cref="AnimationSegmentPlay{TSegment, TFrame, TEvent, TBeat, TShown}.PlayToNextFrame(Animation{TSegment}, ref AnimationPlace, Action{TEvent})"/>
    /// plays an animation with events.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The animation passes every beat without showing a frame, or ends a play of a repeated phase it stands in with no
    /// plays left.
    /// </exception>
    internal static TShown PlayToNextFrame<TSegment, TFrame, TBeat, TShown>(
        this AnimationSegmentPlay<TSegment, TFrame, NoAnimationCue, TBeat, TShown> segments, Animation<TSegment> animation, ref AnimationPlace place)
        where TSegment : struct
        where TFrame : struct
        where TBeat : AnimationBeat<TFrame, NoAnimationCue>
        where TShown : AnimationShown<TSegment, TFrame> =>
        Ensure.NotNull(segments).PlayToNextFrame(animation, ref place, static (NoAnimationCue _) => { });

    /// <summary>
    /// Plays <paramref name="animation"/> on from <paramref name="place"/> to the next frame it shows, as the other overload
    /// does, leaving in <paramref name="animation"/> the animation the play stands in.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The animation passes every beat without showing a frame, or ends a play of a repeated phase it stands in with no
    /// plays left.
    /// </exception>
    internal static TShown PlayToNextFrame<TSegment, TFrame, TBeat, TShown>(
        this AnimationSegmentPlay<TSegment, TFrame, NoAnimationCue, TBeat, TShown> segments, ref Animation<TSegment> animation, ref AnimationPlace place)
        where TSegment : struct
        where TFrame : struct
        where TBeat : AnimationBeat<TFrame, NoAnimationCue>
        where TShown : AnimationShown<TSegment, TFrame> =>
        Ensure.NotNull(segments).PlayToNextFrame(ref animation, ref place, static (NoAnimationCue _) => null);
}
