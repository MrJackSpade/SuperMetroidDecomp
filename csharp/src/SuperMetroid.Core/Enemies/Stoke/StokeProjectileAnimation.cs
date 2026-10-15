using SuperMetroid.Core.Enemies.Stoke.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>Stoke projectile's typed animation.</summary>
internal sealed class StokeProjectileAnimation(StokeProjectileProgram program)
    : EnemyProjectileProgramAnimation<StokeProjectileProgram, StokeProjectileFrame, EnemyProjectileCommonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<StokeProjectileProgram, StokeProjectileFrame, EnemyProjectileCommonCue> Programs => StokeProjectileAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFramePresentation Present(StokeProjectileFrame frame) =>
        EnemyProjectileFramePresentation.Installed(StokeProjectileFrameDefinitions.PresentationKey(frame));
}
