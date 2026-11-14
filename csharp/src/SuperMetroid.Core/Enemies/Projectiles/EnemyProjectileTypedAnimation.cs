using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile animation that plays a family's typed animations (#1600): frames with durations, frames held for good
/// (#1607), events, counted phases and a hold. Each event goes to the behaviour bound to the projectile (#1760), which
/// carries out its family's events as <see cref="IEnemyProjectileCues{TCue}"/>, or as
/// <see cref="IEnemyProjectileBranchingCues{TSegment, TCue}"/> when its reaction continues the play in the animation
/// starting at another segment, as a conditional jump does; the engine carries out the shared
/// <see cref="EnemyProjectileCommonCue"/> events. Counted phases count the projectile's
/// <see cref="EnemyProjectile.LoopCountdown"/>.
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

    /// <summary>The animation playing and where it stands; an imported projectile resumes where its recording stood.</summary>
    internal AnimationPlayback<TSegment> Playback { get; set; }

    /// <summary>
    /// The segment the animation stands in: before its first play, or in an animation of no phase, the segment it starts
    /// at; otherwise the segment its playing phase plays, the one its next play shows a frame of, or rests in.
    /// </summary>
    internal TSegment PlayingSegment => Playback.Place == AnimationPlace.StartOfAnimation || Playback.Animation.Phases.Count == 0
        ? StartingAt.Single(pair => ReferenceEquals(pair.Value, Playback.Animation)).Key
        : Playback.Animation.Phases[Playback.Place.Phase].Segment;

    /// <summary>The family's segments.</summary>
    private protected abstract AnimationSegments<TSegment, TFrame, TEvent> Segments { get; }

    /// <summary>The family's animations, by the segment each starts at.</summary>
    private protected abstract IReadOnlyDictionary<TSegment, Animation<TSegment>> StartingAt { get; }

    /// <summary>The projectile frame <paramref name="frame"/> shows.</summary>
    private protected abstract EnemyProjectileFrameId Present(TFrame frame);

    /// <summary>Plays the animation on to its next frame, raising each event in the projectile's bound behaviour.</summary>
    private protected sealed override EnemyProjectileFrameStep NextFrame(EnemyProjectileCueContext context, EnemyProjectileEngine projectiles)
    {
        EnemyProjectile projectile = context.Projectile;
        Animation<TSegment> animation = Playback.Animation;
        AnimationPlace place = Playback.Place;
        WrappingCountdown repeats = projectile.LoopCountdown;
        AnimationShown<TSegment, TFrame> shown = Segments.PlayToNextFrame(ref animation, ref place, ref repeats,
            raised => RunCue(context, projectiles, raised) is { } target
                ? new AnimationContinuation<TSegment>(StartingAt[target], AnimationPlace.StartOfAnimation)
                : null);
        Playback = new AnimationPlayback<TSegment>(animation, place);
        // A projectile an event deleted keeps its cleared countdown.
        if (projectile.Animation == this)
            projectile.LoopCountdown = repeats;
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
    /// Carries out <paramref name="cue"/>: a shared cue in the engine, any other in the behaviour bound to the projectile.
    /// Returns the segment a branching cue continues the animation at, or null to run on.
    /// </summary>
    /// <exception cref="InvalidOperationException">The projectile's behaviour does not handle the family's cues.</exception>
    private static TSegment? RunCue(EnemyProjectileCueContext context, EnemyProjectileEngine projectiles, TEvent cue)
    {
        if (cue is EnemyProjectileCommonCue common)
        {
            projectiles.ApplyCommonCue(context.Projectile, common);
            return null;
        }
        switch (context.Projectile.Behavior)
        {
            case IEnemyProjectileBranchingCues<TSegment, TEvent> branching:
                return branching.RunCue(context, cue);
            case IEnemyProjectileCues<TEvent> plain:
                plain.RunCue(context, cue);
                return null;
            default:
                throw new InvalidOperationException(
                    $"Enemy projectile {context.Projectile.Kind} in place {context.Projectile.Place} has no behaviour that carries out {typeof(TEvent).Name}.{cue}.");
        }
    }
}
