using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>A Crocomire projectile's typed animation: its mouth projectile, bridge fragment, spike-wall piece or shot.</summary>
/// <param name="segment">The animation to play.</param>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
internal sealed class CrocomireProjectileAnimation(IEnemyProjectileCues<CrocomireProjectileCue> owner, CrocomireProjectileSegment segment)
    : EnemyProjectileFrameAnimation<CrocomireProjectileSegment, CrocomireProjectileCue>(segment, CrocomireAnimations.ProjectileStartingWith(segment), EnemyProjectileCueRunner<CrocomireProjectileSegment, CrocomireProjectileCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<CrocomireProjectileSegment, EnemyProjectileFrameId, CrocomireProjectileCue> Segments =>
        CrocomireAnimations.ProjectileSegments;
}
