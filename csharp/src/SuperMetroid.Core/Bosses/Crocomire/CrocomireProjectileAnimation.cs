using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>A Crocomire projectile's typed animation: its mouth projectile, bridge fragment, spike-wall piece or shot.</summary>
/// <param name="segment">The animation to play.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class CrocomireProjectileAnimation(CrocomireProjectileSegment segment, IEnemyProjectileCues<CrocomireProjectileCue> cues)
    : EnemyProjectileFrameAnimation<CrocomireProjectileSegment, CrocomireProjectileCue>(CrocomireAnimations.ProjectileStartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<CrocomireProjectileSegment, EnemyProjectileFrameId, CrocomireProjectileCue> Segments =>
        CrocomireAnimations.ProjectileSegments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<CrocomireProjectileSegment, Animation<CrocomireProjectileSegment>> StartingAt =>
        CrocomireAnimations.ProjectileStartingAt;
}
