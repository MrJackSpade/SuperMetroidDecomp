using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Who carries out the events a typed projectile animation raises (#1760, #2571), bound to the animation when its owner
/// creates it: the family's cue owner (<see cref="IEnemyProjectileCues{TCue}"/>, or
/// <see cref="IEnemyProjectileBranchingCues{TSegment, TCue}"/> when its reaction continues the play elsewhere), the engine
/// for a family whose events are only the shared <see cref="EnemyProjectileCommonCue"/> events, or nobody for a family
/// whose timelines raise none. The animation runs its runner, so no event is routed by the type of the projectile's
/// behaviour.
/// </summary>
/// <typeparam name="TSegment">The family's segment identity.</typeparam>
/// <typeparam name="TEvent">The family's events.</typeparam>
internal abstract partial class EnemyProjectileCueRunner<TSegment, TEvent>
    where TSegment : struct
    where TEvent : notnull
{
    /// <summary>
    /// Plays <paramref name="animation"/> on from <paramref name="place"/> to its next frame through
    /// <paramref name="segments"/>, carrying out each event the play raises on the projectile of <paramref name="context"/>:
    /// a runner whose owner may continue the play elsewhere hands it the place just past each event, and one whose owner
    /// runs on hands it only the event. On return <paramref name="animation"/> and <paramref name="place"/> are where the
    /// next play resumes.
    /// </summary>
    /// <typeparam name="TFrame">The family's frame identity.</typeparam>
    internal abstract AnimationShown<TSegment, TFrame> PlayToNextFrame<TFrame>(AnimationSegments<TSegment, TFrame, TEvent> segments,
        EnemyProjectileCueContext context, ref Animation<TSegment> animation, ref AnimationPlace place)
        where TFrame : struct;

    /// <summary>The runner that hands each event to <paramref name="owner"/>, whose reaction never continues the play elsewhere.</summary>
    internal static EnemyProjectileCueRunner<TSegment, TEvent> By(IEnemyProjectileCues<TEvent> owner) => new ByOwner(Ensure.NotNull(owner));

    /// <summary>The runner that hands each event to <paramref name="owner"/>, whose reaction may continue the play elsewhere.</summary>
    internal static EnemyProjectileCueRunner<TSegment, TEvent> By(IEnemyProjectileBranchingCues<TSegment, TEvent> owner) =>
        new ByBranchingOwner(Ensure.NotNull(owner));

    /// <summary>Hands each event to a cue owner that runs on.</summary>
    /// <param name="owner">The family's cue owner.</param>
    private sealed partial class ByOwner(IEnemyProjectileCues<TEvent> owner) : EnemyProjectileCueRunner<TSegment, TEvent>
    {
        /// <inheritdoc/>
        internal override AnimationShown<TSegment, TFrame> PlayToNextFrame<TFrame>(AnimationSegments<TSegment, TFrame, TEvent> segments,
            EnemyProjectileCueContext context, ref Animation<TSegment> animation, ref AnimationPlace place) =>
            Ensure.NotNull(segments).PlayToNextFrame(ref animation, ref place, cue =>
            {
                owner.RunCue(context, cue);
                return null;
            });
    }

    /// <summary>Hands each event to a cue owner that may continue the play elsewhere.</summary>
    /// <param name="owner">The family's cue owner.</param>
    private sealed partial class ByBranchingOwner(IEnemyProjectileBranchingCues<TSegment, TEvent> owner) : EnemyProjectileCueRunner<TSegment, TEvent>
    {
        /// <inheritdoc/>
        internal override AnimationShown<TSegment, TFrame> PlayToNextFrame<TFrame>(AnimationSegments<TSegment, TFrame, TEvent> segments,
            EnemyProjectileCueContext context, ref Animation<TSegment> animation, ref AnimationPlace place) =>
            Ensure.NotNull(segments).PlayToNextFrame(ref animation, ref place, (cue, pastCue) => owner.RunCue(context, cue, pastCue));
    }
}
