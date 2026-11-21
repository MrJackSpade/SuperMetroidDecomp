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

    /// <summary>The family's segments, which never pause and may hide.</summary>
    private protected abstract IUnpausedAnimationPlay<TSegment, TFrame, TEvent> Segments { get; }

    /// <summary>
    /// Plays <paramref name="animation"/> on from <paramref name="place"/> to its next frame through <see cref="Segments"/>,
    /// carrying out each event the play raises on the projectile of <paramref name="context"/> if the timeline raises any.
    /// </summary>
    private protected abstract IUnpausedHidingAnimationShown<TSegment, TFrame> PlayToNextFrame(EnemyProjectileCueContext context,
        ref Animation<TSegment> animation, ref AnimationPlace place);

    /// <summary>The family's animations, by the segment each starts at.</summary>
    private protected abstract IEnumerable<KeyValuePair<TSegment, Animation<TSegment>>> StartingAt { get; }

    /// <summary>
    /// The segment <paramref name="started"/>, one of the animations the family's owners start, starts at: an animation of
    /// <see cref="StartingAt"/> at its key, and one an owner builds for itself, which the table does not key (the n00b
    /// tube's shard flights, #2044, Torizo's forming drools, the Tourian Chozo's dust, a death's run, the save station's
    /// electricity), at its first phase's segment.
    /// </summary>
    private protected TSegment SegmentStartedAt(Animation<TSegment> started)
    {
        Ensure.NotNull(started);
        return StartingAt.Any(keyed => ReferenceEquals(keyed.Value, started))
            ? StartingAt.Single(pair => ReferenceEquals(pair.Value, started)).Key
            : started.Phases[0].Segment;
    }

    /// <summary>The projectile frame <paramref name="frame"/> shows.</summary>
    private protected abstract EnemyProjectileFrameId Present(TFrame frame);

    /// <summary>
    /// Where <paramref name="frame"/>'s picture is drawn from the projectile's place (#2557): at the place itself, unless the
    /// family moves one picture about as it plays (the save-station electricity's band on its sweep).
    /// </summary>
    private protected virtual Offset16<RoomPixels> Placement(TFrame frame) => Offset16<RoomPixels>.Zero;

    /// <summary>Plays the animation on to its next frame.</summary>
    private protected sealed override EnemyProjectileFrameStep NextFrame(EnemyProjectileCueContext context)
    {
        Animation<TSegment> animation = Playback.Animation;
        AnimationPlace place = Playback.Place;
        IUnpausedHidingAnimationShown<TSegment, TFrame> shown = PlayToNextFrame(context, ref animation, ref place);
        Playback = new AnimationPlayback<TSegment>(animation, place);
        return shown.Match(NextFrameHidingCases, this);
    }

    /// <summary>
    /// What <see cref="NextFrame"/> makes of each outcome of a projectile animation, which never pauses, built once (#2553): a frame
    /// shows for its duration or for good, a rest holds the timeline, and the animation's end deletes the projectile, as the
    /// game's projectile animation deletes it at its end.
    /// </summary>
    private static UnpausedAnimationShownCases<TSegment, TFrame, EnemyProjectileTimelineAnimation<TSegment, TFrame, TEvent>, EnemyProjectileFrameStep> NextFrameCases { get; } = new(
        Frame: static (shownFrame, timeline) => new EnemyProjectileFrameStep.Show(timeline.Present(shownFrame.Shown), shownFrame.Duration, timeline.Placement(shownFrame.Shown)),
        HeldFrame: static (held, timeline) => new EnemyProjectileFrameStep.ShowForGood(timeline.Present(held.Shown), timeline.Placement(held.Shown)),
        Held: static (_, _) => EnemyProjectileFrameStep.Rests,
        Ended: static (_, _) => EnemyProjectileFrameStep.Ends);

    /// <summary>
    /// What <see cref="NextFrame"/> makes of a hidden stretch, which hides the projectile for its updates (#2558), and of every
    /// other outcome, built once.
    /// </summary>
    private static UnpausedHidingAnimationShownCases<TSegment, TFrame, EnemyProjectileTimelineAnimation<TSegment, TFrame, TEvent>, EnemyProjectileFrameStep> NextFrameHidingCases { get; } = new(
        Shown: static (shown, timeline) => shown.Match(NextFrameCases, timeline),
        Hidden: static (hidden, _) => new EnemyProjectileFrameStep.Hide(hidden.Duration));
}
