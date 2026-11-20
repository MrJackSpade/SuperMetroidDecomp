using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile animation that plays a family's typed animations (#1600): frames with durations, frames held for good
/// (#1607), events, repeated phases and a hold. Each event goes to the behaviour bound to the projectile (#1760), which
/// carries out its family's events as <see cref="IEnemyProjectileCues{TCue}"/>, or as
/// <see cref="IEnemyProjectileBranchingCues{TSegment, TCue}"/> when its reaction continues the play elsewhere, as a
/// conditional change to another animation or a conditional replay of the cue's run does; the engine carries out the shared
/// <see cref="EnemyProjectileCommonCue"/> events. Repeated phases count their plays in the playback (#2006).
/// </summary>
/// <typeparam name="TSegment">The family's segment identity.</typeparam>
/// <typeparam name="TFrame">The family's frame identity.</typeparam>
/// <typeparam name="TEvent">The family's events.</typeparam>
internal abstract class EnemyProjectileTypedAnimation<TSegment, TFrame, TEvent> : EnemyProjectileAnimation
    where TSegment : struct
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>Starts <paramref name="animation"/> before its first play.</summary>
    private protected EnemyProjectileTypedAnimation(Animation<TSegment> animation) =>
        Playback = new AnimationPlayback<TSegment>(Ensure.NotNull(animation), AnimationPlace.StartOfAnimation);

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
            case AnimationShown<TSegment, TFrame>.Hidden hidden:
                return new EnemyProjectileFrameStep.Hide(hidden.Duration);
            case AnimationShown<TSegment, TFrame>.Paused:
            case AnimationShown<TSegment, TFrame>.Ended:
            default:
                throw new InvalidOperationException($"A projectile animation ended an update with {shown}.");
        }
    }

    /// <summary>
    /// Carries out <paramref name="cue"/>: a shared cue in the engine, any other in the behaviour bound to the projectile.
    /// Returns where a branching cue continues the play from <paramref name="pastCue"/>, the place just past the cue, or null
    /// to run on.
    /// </summary>
    /// <exception cref="InvalidOperationException">The projectile's behaviour does not handle the family's cues.</exception>
    private static AnimationContinuation<TSegment>? RunCue(EnemyProjectileCueContext context, EnemyProjectileEngine projectiles, TEvent cue,
        AnimationPlayback<TSegment> pastCue)
    {
        if (cue is EnemyProjectileCommonCue common)
        {
            projectiles.ApplyCommonCue(context.Projectile, common);
            return null;
        }
        switch (context.Projectile.Behavior)
        {
            case IEnemyProjectileBranchingCues<TSegment, TEvent> branching:
                return branching.RunCue(context, cue, pastCue);
            case IEnemyProjectileCues<TEvent> plain:
                plain.RunCue(context, cue);
                return null;
            default:
                throw new InvalidOperationException(
                    $"Enemy projectile {context.Projectile.Kind} in place {context.Projectile.Place} has no behaviour that carries out {typeof(TEvent).Name}.{cue}.");
        }
    }
}
