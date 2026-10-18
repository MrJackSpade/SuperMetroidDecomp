using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>A Tourian statue projectile's typed animation.</summary>
internal sealed class TourianStatueProjectileAnimation(TourianStatueProjectileProgram program)
    : EnemyProjectileProgramAnimation<TourianStatueProjectileProgram, TourianStatueProjectileFrame, TourianStatueProjectileCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<TourianStatueProjectileProgram, TourianStatueProjectileFrame, TourianStatueProjectileCue> Programs =>
        TourianStatueProjectileAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFramePresentation Present(TourianStatueProjectileFrame frame) =>
        EnemyProjectileFramePresentation.Installed(TourianStatueProjectileFrameDefinitions.PresentationKeys[frame]);
}
