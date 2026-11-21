using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>Kago bug's typed animation.</summary>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
/// <param name="segment">Where the animation starts.</param>
internal sealed class KagoBugProjectileAnimation(IEnemyProjectileCues<KagoBugProjectileCue> owner, KagoBugProjectileSegment segment)
    : EnemyProjectileFrameAnimation<KagoBugProjectileSegment, KagoBugProjectileCue>(segment, KagoBugProjectileAnimations.StartingAt[segment], EnemyProjectileCueRunner<KagoBugProjectileSegment, KagoBugProjectileCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<KagoBugProjectileSegment, EnemyProjectileFrameId, KagoBugProjectileCue> Segments => KagoBugProjectileAnimations.Segments;
}
