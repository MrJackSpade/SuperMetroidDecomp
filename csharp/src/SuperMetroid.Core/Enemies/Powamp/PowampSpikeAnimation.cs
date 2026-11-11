using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Powamp.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>Powamp spike's typed animation.</summary>
internal sealed class PowampSpikeAnimation(PowampSpikeProgram program)
    : EnemyProjectileTypedAnimation<PowampSpikeProgram, PowampSpikeFrame, EnemyProjectileCommonCue>(PowampSpikeAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<PowampSpikeProgram, PowampSpikeFrame, EnemyProjectileCommonCue> Segments => PowampSpikeAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<PowampSpikeProgram, Animation<PowampSpikeProgram>> StartingAt => PowampSpikeAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(PowampSpikeFrame frame) =>
        PowampSpikeFrameDefinitions.ShownProjectileFrames[frame];
}
