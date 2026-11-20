using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile animation that plays a family's typed timeline (#1600): frames with durations, frames held for good
/// (#1607), repeated phases and a hold. A timeline that raises events is an <see cref="EnemyProjectileTypedAnimation{TSegment,
/// TFrame, TEvent}"/>, whose events go to the cue runner its owner bound; one that raises none is an
/// <see cref="EnemyProjectileSilentAnimation{TSegment, TFrame}"/>, which has no runner (#2571). Repeated phases count their
/// plays in the playback (#2006).
/// </summary>
/// <typeparam name="TSegment">The family's segment identity.</typeparam>
/// <typeparam name="TFrame">The family's frame identity.</typeparam>
/// <typeparam name="TEvent">The family's events.</typeparam>
internal abstract class EnemyProjectileTimelineAnimation<TSegment, TFrame, TEvent> : EnemyProjectileAnimation
    where TSegment : struct
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>Starts <paramref name="animation"/> before its first play.</summary>
    private protected EnemyProjectileTimelineAnimation(Animation<TSegment> animation) =>
        Playback = new AnimationPlayback<TSegment>(Ensure.NotNull(animation), AnimationPlace.StartOfAnimation);

    /// <summary>The animation playing and where it stands.</summary>
    private protected AnimationPlayback<TSegment> Playback { private get; set; }

    /// <summary>
    /// The segment the animation stands in: before its first play, or in an animation of no phase, the segment it starts
    /// at; otherwise the segment its playing phase plays, the one its next play shows a frame of, or rests in.
    /// </summary>
    internal TSegment PlayingSegment => Playback.Place == AnimationPlace.StartOfAnimation || Playback.Animation.Phases.Count == 0
        ? SegmentStartedAt(Playback.Animation)
        : Playback.Animation.Phases[Playback.Place.Phase].Segment;

    /// <summary>The family's segments.</summary>
    private protected abstract AnimationSegments<TSegment, TFrame, TEvent> Segments { get; }

    /// <summary>
    /// Plays <paramref name="animation"/> on from <paramref name="place"/> to its next frame through <see cref="Segments"/>,
    /// carrying out each event the play raises on the projectile of <paramref name="context"/> if the timeline raises any.
    /// </summary>
    private protected abstract AnimationShown<TSegment, TFrame> PlayToNextFrame(EnemyProjectileCueContext context,
        ref Animation<TSegment> animation, ref AnimationPlace place);

    /// <summary>The family's animations, by the segment each starts at.</summary>
    private protected abstract IEnumerable<KeyValuePair<TSegment, Animation<TSegment>>> StartingAt { get; }

    /// <summary>
    /// The segment <paramref name="started"/>, one of the family's animations, starts at: its key in <see cref="StartingAt"/>.
    /// A family whose owners start some animations by their own identity (the n00b tube's shards, #2044) names theirs.
    /// </summary>
    private protected virtual TSegment SegmentStartedAt(Animation<TSegment> started) =>
        StartingAt.Single(pair => ReferenceEquals(pair.Value, started)).Key;

    /// <summary>The projectile frame <paramref name="frame"/> shows.</summary>
    private protected abstract EnemyProjectileFrameId Present(TFrame frame);

    /// <summary>Plays the animation on to its next frame.</summary>
    private protected sealed override EnemyProjectileFrameStep NextFrame(EnemyProjectileCueContext context)
    {
        Animation<TSegment> animation = Playback.Animation;
        AnimationPlace place = Playback.Place;
        AnimationShown<TSegment, TFrame> shown = PlayToNextFrame(context, ref animation, ref place);
        Playback = new AnimationPlayback<TSegment>(animation, place);
        return shown.Match(NextFrameCases, this);
    }

    /// <summary>
    /// What <c>NextFrame</c> makes of each <see cref="AnimationShown{TSegment, TFrame}"/>, built once (#2553): a frame shows
    /// for its duration or for good, a pause keeps the frame shown for its duration, a rest holds the timeline, and the
    /// animation's end deletes the projectile, as the common end deletes an enemy.
    /// </summary>
    private static AnimationShownCases<TSegment, TFrame, EnemyProjectileTimelineAnimation<TSegment, TFrame, TEvent>, EnemyProjectileFrameStep> NextFrameCases { get; } = new(
        Frame: static (shownFrame, timeline) => new EnemyProjectileFrameStep.Show(timeline.Present(shownFrame.Shown), shownFrame.Duration),
        HeldFrame: static (held, timeline) => new EnemyProjectileFrameStep.ShowForGood(timeline.Present(held.Shown)),
        Paused: static (paused, _) => new EnemyProjectileFrameStep.Wait(paused.Duration),
        Held: static (_, _) => EnemyProjectileFrameStep.Rests,
        Ended: static (_, _) => EnemyProjectileFrameStep.Ends);
}
