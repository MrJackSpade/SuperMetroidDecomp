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
internal abstract partial class EnemyProjectileTimelineAnimation<TSegment, TFrame, TEvent> : EnemyProjectileAnimation
    where TSegment : struct
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>Starts <paramref name="animation"/>, which starts at <paramref name="startsAt"/>, before its first play.</summary>
    /// <param name="startsAt">
    /// The segment the animation starts at: the one the family's table keys it by, or the first phase's of an animation an
    /// owner builds for itself (the n00b tube's shard flights, #2044, Torizo's forming drools, the Tourian Chozo's dust, a
    /// death's run, the save station's electricity).
    /// </param>
    /// <param name="animation">The animation to play.</param>
    private protected EnemyProjectileTimelineAnimation(TSegment startsAt, Animation<TSegment> animation) => Start(startsAt, animation);

    /// <summary>The animation playing and where it stands.</summary>
    internal AnimationPlayback<TSegment> Playback { get; set; }

    /// <summary>The segment the playing animation starts at, which its owner names as it starts it.</summary>
    private TSegment StartedAt { get; set; }

    /// <summary>
    /// The segment the animation stands in: before its first play, or in an animation of no phase, the segment it starts
    /// at; otherwise the segment its playing phase plays, the one its next play shows a frame of, or rests in.
    /// </summary>
    internal TSegment PlayingSegment => Playback.Place == AnimationPlace.StartOfAnimation || Playback.Animation.Phases.Count == 0
        ? StartedAt
        : Playback.Animation.Phases[Playback.Place.Phase].Segment;

    /// <summary>Starts <paramref name="animation"/>, which starts at <paramref name="startsAt"/>, before its first play.</summary>
    private protected void Start(TSegment startsAt, Animation<TSegment> animation)
    {
        StartedAt = startsAt;
        Playback = new AnimationPlayback<TSegment>(Ensure.NotNull(animation), AnimationPlace.StartOfAnimation);
    }

    /// <summary>The family's segments.</summary>
    private protected abstract AnimationSegments<TSegment, TFrame, TEvent> Segments { get; }

    /// <summary>
    /// Plays <paramref name="animation"/> on from <paramref name="place"/> to its next frame through <see cref="Segments"/>,
    /// carrying out each event the play raises on the projectile of <paramref name="context"/> if the timeline raises any.
    /// </summary>
    private protected abstract AnimationShown<TSegment, TFrame> PlayToNextFrame(EnemyProjectileCueContext context,
        ref Animation<TSegment> animation, ref AnimationPlace place);

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
        AnimationShown<TSegment, TFrame> shown = PlayToNextFrame(context, ref animation, ref place);
        Playback = new AnimationPlayback<TSegment>(animation, place);
        switch (shown)
        {
            case AnimationShown<TSegment, TFrame>.Frame shownFrame:
                return new EnemyProjectileFrameStep.Show(Present(shownFrame.Shown), shownFrame.Duration, Placement(shownFrame.Shown));
            case AnimationShown<TSegment, TFrame>.HeldFrame held:
                return new EnemyProjectileFrameStep.ShowForGood(Present(held.Shown), Placement(held.Shown));
            case AnimationShown<TSegment, TFrame>.Held:
                return EnemyProjectileFrameStep.Rests;
            case AnimationShown<TSegment, TFrame>.Hidden hidden:
                return new EnemyProjectileFrameStep.Hide(hidden.Duration);
            case AnimationShown<TSegment, TFrame>.Ended:
                // A projectile animation that ends deletes its projectile.
                return EnemyProjectileFrameStep.Deletes;
            case AnimationShown<TSegment, TFrame>.Paused:
            default:
                throw new InvalidOperationException($"A projectile animation ended an update with {shown}.");
        }
    }
}
