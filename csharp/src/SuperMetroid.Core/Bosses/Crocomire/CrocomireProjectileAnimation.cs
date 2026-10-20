using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>A Crocomire projectile's typed animation: its mouth projectile, bridge fragment, spike-wall piece or shot.</summary>
/// <param name="kind">The program to play.</param>
internal sealed class CrocomireProjectileAnimation(CrocomireProjectileFrameKind kind)
    : EnemyProjectileProgramAnimation<CrocomireProjectileFrameKind, CrocomireProjectileFrame, CrocomireProjectileCue>(kind)
{

    /// <inheritdoc/>
    private protected override AnimationProgramSet<CrocomireProjectileFrameKind, CrocomireProjectileFrame, CrocomireProjectileCue> Programs =>
        CrocomireAnimations.ProjectilePrograms;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(CrocomireProjectileFrame frame) =>
        CrocomireProjectileVisualDefinitions.ShownProjectileFrames[frame];
}
