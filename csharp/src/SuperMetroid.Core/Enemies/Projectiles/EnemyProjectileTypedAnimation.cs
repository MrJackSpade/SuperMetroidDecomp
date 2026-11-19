using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile animation that plays a family's typed animations (#1600): frames with durations, frames held for good
/// (#1607), events, repeated phases and a hold. Each event goes to the cue runner its owner bound it to when it created
/// it (#1760, #2571, <see cref="EnemyProjectileCueRunner{TSegment, TEvent}"/>): the family's cue owner, which may continue the
/// play elsewhere, as a conditional change to another animation or a conditional replay of the cue's run does, or the
/// engine for the shared <see cref="EnemyProjectileCommonCue"/> events. Repeated phases count their plays in the playback
/// (#2006).
/// </summary>
/// <typeparam name="TSegment">The family's segment identity.</typeparam>
/// <typeparam name="TFrame">The family's frame identity.</typeparam>
/// <typeparam name="TEvent">The family's events.</typeparam>
internal abstract class EnemyProjectileTypedAnimation<TSegment, TFrame, TEvent> : EnemyProjectileAnimation
    where TSegment : struct
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>Who carries out the events the animation raises, bound when its owner created it (#2571).</summary>
    private readonly EnemyProjectileCueRunner<TSegment, TEvent> _cues;

    /// <summary>Starts <paramref name="animation"/> before its first play, its events carried out by <paramref name="cues"/>.</summary>
    private protected EnemyProjectileTypedAnimation(Animation<TSegment> animation, EnemyProjectileCueRunner<TSegment, TEvent> cues)
    {
        Playback = new AnimationPlayback<TSegment>(Ensure.NotNull(animation), AnimationPlace.StartOfAnimation);
        _cues = Ensure.NotNull(cues);
    }

    /// <summary>The animation playing and where it stands.</summary>
    internal AnimationPlayback<TSegment> Playback { get; set; }

    /// <summary>
    /// The segment the animation stands in: before its first play, or in an animation of no phase, the segment it starts
    /// at; otherwise the segment its playing phase plays, the one its next play shows a frame of, or rests in.
    /// </summary>
    internal TSegment PlayingSegment => Playback.Place == AnimationPlace.StartOfAnimation || Playback.Animation.Phases.Count == 0
        ? SegmentStartedAt(Playback.Animation)
        : Playback.Animation.Phases[Playback.Place.Phase].Segment;

    /// <summary>The family's segments.</summary>
    private protected abstract AnimationSegments<TSegment, TFrame, TEvent> Segments { get; }

    /// <summary>The family's animations, by the segment each starts at.</summary>
    private protected abstract IReadOnlyDictionary<TSegment, Animation<TSegment>> StartingAt { get; }

    /// <summary>
    /// The segment <paramref name="started"/>, one of the family's animations, starts at: its key in <see cref="StartingAt"/>.
    /// A family whose owners start some animations by their own identity (the n00b tube's shards, #2044) names theirs.
    /// </summary>
    private protected virtual TSegment SegmentStartedAt(Animation<TSegment> started) =>
        StartingAt.Single(pair => ReferenceEquals(pair.Value, started)).Key;

    /// <summary>The projectile frame <paramref name="frame"/> shows.</summary>
    private protected abstract EnemyProjectileFrameId Present(TFrame frame);

    /// <summary>Plays the animation on to its next frame, raising each event in the projectile's bound behaviour.</summary>
    private protected sealed override EnemyProjectileFrameStep NextFrame(EnemyProjectileCueContext context, EnemyProjectileEngine projectiles)
    {
        Animation<TSegment> animation = Playback.Animation;
        AnimationPlace place = Playback.Place;
        AnimationShown<TSegment, TFrame> shown = Segments.PlayToNextFrame(ref animation, ref place,
            (raised, pastCue) => RunCue(context, projectiles, raised, pastCue));
        Playback = new AnimationPlayback<TSegment>(animation, place);
        switch (shown)
        {
            case AnimationShown<TSegment, TFrame>.Frame shownFrame:
                return new EnemyProjectileFrameStep.Show(Present(shownFrame.Shown), shownFrame.Duration);
            case AnimationShown<TSegment, TFrame>.HeldFrame held:
                return new EnemyProjectileFrameStep.ShowForGood(Present(held.Shown));
            case AnimationShown<TSegment, TFrame>.Held:
                return EnemyProjectileFrameStep.Rests;
            case AnimationShown<TSegment, TFrame>.Paused:
            case AnimationShown<TSegment, TFrame>.Ended:
            default:
                throw new InvalidOperationException($"A projectile animation ended an update with {shown}.");
        }
    }

    /// <summary>
    /// Carries out <paramref name="cue"/> through the runner bound to the animation. Returns where a branching cue continues
    /// the play from <paramref name="pastCue"/>, the place just past the cue, or null to run on.
    /// </summary>
    private AnimationContinuation<TSegment>? RunCue(EnemyProjectileCueContext context, EnemyProjectileEngine projectiles, TEvent cue,
        AnimationPlayback<TSegment> pastCue) => _cues.Run(context, projectiles, cue, pastCue);
}
