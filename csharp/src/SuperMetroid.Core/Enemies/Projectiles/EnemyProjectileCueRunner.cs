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
internal abstract class EnemyProjectileCueRunner<TSegment, TEvent>
    where TSegment : struct
{
    /// <summary>
    /// Carries out <paramref name="cue"/>, raised on the context's projectile, returning where the play continues from
    /// <paramref name="pastCue"/>, the place just past the cue, or null to run on.
    /// </summary>
    internal abstract AnimationContinuation<TSegment>? Run(EnemyProjectileCueContext context, EnemyProjectileEngine projectiles,
        TEvent cue, AnimationPlayback<TSegment> pastCue);

    /// <summary>The runner that hands each event to <paramref name="owner"/>, whose reaction never continues the play elsewhere.</summary>
    internal static EnemyProjectileCueRunner<TSegment, TEvent> By(IEnemyProjectileCues<TEvent> owner) => new ByOwner(Ensure.NotNull(owner));

    /// <summary>The runner that hands each event to <paramref name="owner"/>, whose reaction may continue the play elsewhere.</summary>
    internal static EnemyProjectileCueRunner<TSegment, TEvent> By(IEnemyProjectileBranchingCues<TSegment, TEvent> owner) =>
        new ByBranchingOwner(Ensure.NotNull(owner));

    /// <summary>Hands each event to a cue owner that runs on.</summary>
    /// <param name="owner">The family's cue owner.</param>
    private sealed class ByOwner(IEnemyProjectileCues<TEvent> owner) : EnemyProjectileCueRunner<TSegment, TEvent>
    {
        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "SME6426", Justification = "A cue runner overrides the one shared step the runner of every family takes; the branching runners read the engine and the playback past the cue, this one runs on without them.")]
        internal override AnimationContinuation<TSegment>? Run(EnemyProjectileCueContext context, EnemyProjectileEngine projectiles,
            TEvent cue, AnimationPlayback<TSegment> pastCue)
        {
            owner.RunCue(context, cue);
            return null;
        }
    }

    /// <summary>Hands each event to a cue owner that may continue the play elsewhere.</summary>
    /// <param name="owner">The family's cue owner.</param>
    private sealed class ByBranchingOwner(IEnemyProjectileBranchingCues<TSegment, TEvent> owner) : EnemyProjectileCueRunner<TSegment, TEvent>
    {
        /// <inheritdoc/>
        internal override AnimationContinuation<TSegment>? Run(EnemyProjectileCueContext context, EnemyProjectileEngine projectiles,
            TEvent cue, AnimationPlayback<TSegment> pastCue) => owner.RunCue(context, cue, pastCue);
    }
}
