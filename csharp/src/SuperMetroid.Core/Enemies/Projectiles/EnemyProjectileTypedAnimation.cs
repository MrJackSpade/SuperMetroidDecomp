using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile animation that plays a family's typed animations (#1600): frames with durations, frames held for good
/// (#1607), events, repeated phases and a hold. Each event goes to the owner of the animation's cues, the behaviour that
/// started it (#1760, #2554), which carries out its family's events as <see cref="IEnemyProjectileCues{TCue}"/>, or as
/// <see cref="IEnemyProjectileBranchingCues{TSegment, TCue}"/> when its reaction continues the play elsewhere, as a
/// conditional change to another animation or a conditional replay of the cue's run does; the engine carries out the shared
/// <see cref="EnemyProjectileCommonCue"/> events of the families that publish only those
/// (<see cref="EnemyProjectileCommonFrameAnimation{TSegment}"/>). A cue reaches only the owner the animation was started
/// with. Repeated phases count their plays in the playback (#2006).
/// </summary>
/// <typeparam name="TSegment">The family's segment identity.</typeparam>
/// <typeparam name="TFrame">The family's frame identity.</typeparam>
/// <typeparam name="TEvent">The family's events.</typeparam>
internal abstract class EnemyProjectileTypedAnimation<TSegment, TFrame, TEvent> : EnemyProjectileAnimation
    where TSegment : struct
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>Carries out one of the animation's cues and returns where the play continues, or null to run on.</summary>
    /// <param name="context">The projectile, Samus and the camera of the pass running the animation.</param>
    /// <param name="projectiles">The projectile engine, which carries out the shared cues.</param>
    /// <param name="cue">The cue the animation published.</param>
    /// <param name="pastCue">Where the play stands just past the cue.</param>
    private protected delegate AnimationContinuation<TSegment>? CueRunner(EnemyProjectileCueContext context, EnemyProjectileEngine projectiles,
        TEvent cue, AnimationPlayback<TSegment> pastCue);

    /// <summary>Carries out each of the animation's cues in the owner it was started with.</summary>
    private readonly CueRunner _runCue;

    /// <summary>Starts <paramref name="animation"/> before its first play, its cues carried out by <paramref name="cues"/>.</summary>
    /// <param name="animation">The animation.</param>
    /// <param name="cues">The behaviour that owns the animation's cues.</param>
    private protected EnemyProjectileTypedAnimation(Animation<TSegment> animation, IEnemyProjectileCues<TEvent> cues)
        : this(animation, Plain(Ensure.NotNull(cues)))
    {
    }

    /// <summary>
    /// Starts <paramref name="animation"/> before its first play, its cues carried out by <paramref name="cues"/>, whose
    /// reactions may continue the play elsewhere.
    /// </summary>
    /// <param name="animation">The animation.</param>
    /// <param name="cues">The behaviour that owns the animation's cues.</param>
    private protected EnemyProjectileTypedAnimation(Animation<TSegment> animation, IEnemyProjectileBranchingCues<TSegment, TEvent> cues)
        : this(animation, Branching(Ensure.NotNull(cues)))
    {
    }

    /// <summary>
    /// Starts <paramref name="animation"/> before its first play, its cues carried out by <paramref name="cues"/>, whose
    /// reactions may hand the play over to another family's animation: the animation's own play is then over, and the
    /// projectile plays on in the animation handed to within the same pass (#2554).
    /// </summary>
    /// <param name="animation">The animation.</param>
    /// <param name="cues">The behaviour that owns the animation's cues.</param>
    private protected EnemyProjectileTypedAnimation(Animation<TSegment> animation, IEnemyProjectileHandingOverCues<TEvent> cues)
    {
        Ensure.NotNull(cues);
        Playback = new AnimationPlayback<TSegment>(Ensure.NotNull(animation), AnimationPlace.StartOfAnimation);
        _runCue = (context, _, cue, _) => cues.RunCue(context, cue) is { } next ? HandOverTo(next) : null;
    }

    /// <summary>Starts <paramref name="animation"/> before its first play, its cues carried out by <paramref name="runCue"/>.</summary>
    /// <param name="animation">The animation.</param>
    /// <param name="runCue">Carries out each of the animation's cues.</param>
    private protected EnemyProjectileTypedAnimation(Animation<TSegment> animation, CueRunner runCue)
    {
        Playback = new AnimationPlayback<TSegment>(Ensure.NotNull(animation), AnimationPlace.StartOfAnimation);
        _runCue = Ensure.NotNull(runCue);
    }

    /// <summary>The animation a cue of this play handed the projectile over to, which the play's step goes on in; none before.</summary>
    private EnemyProjectileAnimation? _handedOverTo;

    /// <summary>An animation of no phase that is over at once: the play continues in it to end where a cue handed it over.</summary>
    private static Animation<TSegment> PlayOver { get; } = new([], AnimationEnd.Stops);

    /// <summary>Records <paramref name="next"/> as the animation the play goes on in, and ends this animation's own play.</summary>
    private AnimationContinuation<TSegment> HandOverTo(EnemyProjectileAnimation next)
    {
        _handedOverTo = next;
        return new AnimationContinuation<TSegment>(PlayOver, AnimationPlace.StartOfAnimation);
    }

    /// <summary>Runs each cue in <paramref name="cues"/>, the play running on past it.</summary>
    private static CueRunner Plain(IEnemyProjectileCues<TEvent> cues) => (context, _, cue, _) =>
    {
        cues.RunCue(context, cue);
        return null;
    };

    /// <summary>Runs each cue in <paramref name="cues"/>, which says where the play continues.</summary>
    private static CueRunner Branching(IEnemyProjectileBranchingCues<TSegment, TEvent> cues) =>
        (context, _, cue, pastCue) => cues.RunCue(context, cue, pastCue);

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
        if (_handedOverTo is { } handedOverTo)
            return new EnemyProjectileFrameStep.HandOver(handedOverTo);
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
    /// Carries out <paramref name="cue"/> in the owner the animation was started with. Returns where a branching cue
    /// continues the play from <paramref name="pastCue"/>, the place just past the cue, or null to run on.
    /// </summary>
    private AnimationContinuation<TSegment>? RunCue(EnemyProjectileCueContext context, EnemyProjectileEngine projectiles, TEvent cue,
        AnimationPlayback<TSegment> pastCue) =>
        _runCue(context, projectiles, cue, pastCue);
}
