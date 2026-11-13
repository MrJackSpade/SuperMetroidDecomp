using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>A Crocomire projectile's typed animation: its mouth projectile, bridge fragment, spike-wall piece or shot.</summary>
/// <param name="kind">The animation to play.</param>
internal sealed class CrocomireProjectileAnimation(CrocomireProjectileFrameKind kind)
    : EnemyProjectileTypedAnimation<CrocomireProjectileFrameKind, CrocomireProjectileFrame, CrocomireProjectileCue>(CrocomireAnimations.ProjectileStartingWith(kind))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<CrocomireProjectileFrameKind, CrocomireProjectileFrame, CrocomireProjectileCue> Segments =>
        CrocomireAnimations.ProjectileSegments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<CrocomireProjectileFrameKind, Animation<CrocomireProjectileFrameKind>> StartingAt =>
        CrocomireAnimations.ProjectileStartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(CrocomireProjectileFrame frame) =>
        CrocomireProjectileVisualDefinitions.ShownProjectileFrames[frame];
}
