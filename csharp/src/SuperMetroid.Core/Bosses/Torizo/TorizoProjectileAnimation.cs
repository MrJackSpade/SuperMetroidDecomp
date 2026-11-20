using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// A Torizo projectile's typed animation: one starting at a segment, or one its owner starts by its own name, as a forming
/// drool's (#2411).
/// </summary>
/// <param name="animation">The animation the projectile plays from its start.</param>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
internal sealed class TorizoProjectileAnimation(IEnemyProjectileBranchingCues<TorizoProjectileSegment, TorizoProjectileCue> owner, Animation<TorizoProjectileSegment> animation)
    : EnemyProjectileFrameAnimation<TorizoProjectileSegment, TorizoProjectileCue>(animation, EnemyProjectileCueRunner<TorizoProjectileSegment, TorizoProjectileCue>.By(owner))
{
    /// <summary>A projectile animation starting at <paramref name="segment"/>.</summary>
    /// <param name="owner">The family's cue owner.</param>
    /// <param name="segment">The segment the animation starts at.</param>
    internal TorizoProjectileAnimation(IEnemyProjectileBranchingCues<TorizoProjectileSegment, TorizoProjectileCue> owner, TorizoProjectileSegment segment)
        : this(owner, TorizoProjectileAnimations.StartingWith(segment))
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<TorizoProjectileSegment, EnemyProjectileFrameId, TorizoProjectileCue> Segments => TorizoProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<TorizoProjectileSegment, Animation<TorizoProjectileSegment>> StartingAt => TorizoProjectileAnimations.StartingAt;

    /// <summary>The segment <paramref name="started"/> starts at: its key in <see cref="StartingAt"/>, or a forming drool's first segment.</summary>
    private protected override TorizoProjectileSegment SegmentStartedAt(Animation<TorizoProjectileSegment> started) =>
        StartingAt.Values.Any(keyed => ReferenceEquals(keyed, started)) ? base.SegmentStartedAt(started) : Ensure.NotNull(started).Phases[0].Segment;
}
