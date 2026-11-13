using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile animation that plays a family's typed animations (#1600): frames with durations, frames held for good
/// (#1607), events, counted phases and a hold. Each event goes to the handler its family registered with
/// <see cref="EnemyProjectileHandlerTable.AddCue{TCue}"/>, or with
/// <see cref="EnemyProjectileHandlerTable.AddBranchingCue{TProgram, TCue}"/> when its reaction continues the play in the
/// animation starting at another segment, as a conditional jump does. Counted phases count the projectile's
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

    /// <summary>Plays the animation on to its next frame, raising each event in the family's registered handler.</summary>
    private protected sealed override EnemyProjectileFrameStep NextFrame(EnemyProjectileCueContext context, EnemyProjectileHandlerTable handlers)
    {
        EnemyProjectile projectile = context.Projectile;
        Animation<TSegment> animation = Playback.Animation;
        AnimationPlace place = Playback.Place;
        WrappingCountdown repeats = projectile.LoopCountdown;
        AnimationShown<TSegment, TFrame> shown = Segments.PlayToNextFrame(ref animation, ref place, ref repeats,
            raised => handlers.PublishProgramCue<TSegment, TEvent>(context, raised) is { } target
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
}
