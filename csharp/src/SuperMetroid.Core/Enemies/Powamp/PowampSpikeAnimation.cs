using SuperMetroid.Core.Enemies.Powamp.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>Powamp spike's typed animation.</summary>
internal sealed class PowampSpikeAnimation(PowampSpikeProgram program)
    : EnemyProjectileProgramAnimation<PowampSpikeProgram, PowampSpikeFrame, EnemyProjectileCommonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<PowampSpikeProgram, PowampSpikeFrame, EnemyProjectileCommonCue> Programs => PowampSpikeAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFramePresentation Present(PowampSpikeFrame frame) =>
        EnemyProjectileFramePresentation.Installed(PowampSpikeFrameDefinitions.PresentationKey(frame));
}
