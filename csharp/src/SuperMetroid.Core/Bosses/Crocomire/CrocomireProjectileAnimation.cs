using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>A Crocomire projectile's typed animation: its mouth projectile, bridge fragment, spike-wall piece or shot.</summary>
/// <param name="segment">The animation to play.</param>
internal sealed class CrocomireProjectileAnimation(CrocomireProjectileSegment segment)
    : EnemyProjectileFrameAnimation<CrocomireProjectileSegment, CrocomireProjectileCue>(CrocomireAnimations.ProjectileStartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<CrocomireProjectileSegment, EnemyProjectileFrameId, CrocomireProjectileCue> Segments =>
        CrocomireAnimations.ProjectileSegments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<CrocomireProjectileSegment, Animation<CrocomireProjectileSegment>> StartingAt =>
        CrocomireAnimations.ProjectileStartingAt;
}
