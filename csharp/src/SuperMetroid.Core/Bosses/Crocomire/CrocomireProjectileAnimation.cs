using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>A Crocomire projectile's typed animation: its mouth projectile, bridge fragment, spike-wall piece or shot.</summary>
internal sealed partial class CrocomireProjectileAnimation
    : EnemyProjectileFrameAnimation<CrocomireProjectileSegment, CrocomireProjectileCue>
{
    /// <summary>Creates a <see cref="CrocomireProjectileAnimation"/>.</summary>
    /// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
    /// <param name="segment">The animation to play.</param>
    public CrocomireProjectileAnimation(IEnemyProjectileCues<CrocomireProjectileCue> owner, CrocomireProjectileSegment segment)
        : base(segment, CrocomireAnimations.ProjectileStartingWith(segment), EnemyProjectileCueRunner<CrocomireProjectileSegment, CrocomireProjectileCue>.By(owner))
    {
    }

    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<CrocomireProjectileSegment, EnemyProjectileFrameId, CrocomireProjectileCue> Segments =>
        CrocomireAnimations.ProjectileSegments;
}
