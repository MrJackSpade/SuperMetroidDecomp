using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>Powamp spike's typed animation.</summary>
internal sealed class PowampSpikeAnimation(PowampSpikeSegment segment)
    : EnemyProjectileCommonFrameAnimation<PowampSpikeSegment>(PowampSpikeAnimations.StartingWith(segment), EnemyProjectileCueRunners.Common<PowampSpikeSegment>())
{
    /// <inheritdoc/>
    private protected override AnimationSegments<PowampSpikeSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => PowampSpikeAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<PowampSpikeSegment, Animation<PowampSpikeSegment>> StartingAt => PowampSpikeAnimations.StartingAt;
}
