using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>Powamp spike's typed animation.</summary>
internal sealed class PowampSpikeAnimation(PowampSpikeProgram program)
    : EnemyProjectileFrameAnimation<PowampSpikeProgram, EnemyProjectileCommonCue>(PowampSpikeAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<PowampSpikeProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => PowampSpikeAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<PowampSpikeProgram, Animation<PowampSpikeProgram>> StartingAt => PowampSpikeAnimations.StartingAt;
}
