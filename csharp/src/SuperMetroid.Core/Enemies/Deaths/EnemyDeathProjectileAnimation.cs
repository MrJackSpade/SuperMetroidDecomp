using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Deaths.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>Enemy death and drop's typed animation.</summary>
internal sealed class EnemyDeathProjectileAnimation(EnemyDeathProjectileProgram program)
    : EnemyProjectileProgramAnimation<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue> Programs => EnemyDeathProjectileAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFramePresentation Present(EnemyDeathProjectileFrame frame) =>
        EnemyProjectileFramePresentation.Installed(EnemyDeathProjectileFrameDefinitions.PresentationKey(frame));
}
