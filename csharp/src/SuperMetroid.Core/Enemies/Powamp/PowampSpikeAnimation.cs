using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>Powamp spike's typed animation.</summary>
internal sealed class PowampSpikeAnimation(PowampSpikeSegment segment)
    : EnemyProjectileFrameAnimation<PowampSpikeSegment, EnemyProjectileCommonCue>(PowampSpikeAnimations.StartingWith(segment), EnemyProjectileCueRunners.Common<PowampSpikeSegment>())
{
    /// <inheritdoc/>
    private protected override UnpausedAnimationSegments<PowampSpikeSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => PowampSpikeAnimations.Segments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<PowampSpikeSegment, Animation<PowampSpikeSegment>>> StartingAt => PowampSpikeAnimations.StartingAt;
}
