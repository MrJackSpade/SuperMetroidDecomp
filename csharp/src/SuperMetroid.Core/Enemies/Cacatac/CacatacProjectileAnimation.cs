using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Cacatac.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Cacatac spike's typed animation.</summary>
internal sealed class CacatacProjectileAnimation(CacatacProjectileProgram program)
    : EnemyProjectileProgramAnimation<CacatacProjectileProgram, CacatacProjectileFrame, EnemyProjectileCommonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<CacatacProjectileProgram, CacatacProjectileFrame, EnemyProjectileCommonCue> Programs => CacatacProjectileAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFramePresentation Present(CacatacProjectileFrame frame) =>
        EnemyProjectileFramePresentation.Installed(CacatacProjectileFrameDefinitions.PresentationKey(frame));
}
