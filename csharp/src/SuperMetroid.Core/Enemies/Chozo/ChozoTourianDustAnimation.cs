using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Chozo/Tourian dust's typed animation.</summary>
internal sealed class ChozoTourianDustAnimation(ChozoTourianDustProgram program)
    : EnemyProjectileProgramAnimation<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue> Programs => ChozoTourianDustAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFramePresentation Present(ChozoTourianDustFrame frame) =>
        EnemyProjectileFramePresentation.Installed(ChozoTourianDustFrameDefinitions.PresentationKey(frame));
}
