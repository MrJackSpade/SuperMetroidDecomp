using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>Powamp spike's typed animation.</summary>
internal sealed partial class PowampSpikeAnimation
    : EnemyProjectileFrameAnimation<PowampSpikeSegment, EnemyProjectileCommonCue>
{
    /// <summary>Creates a <see cref="PowampSpikeAnimation"/>.</summary>
    /// <param name="segment">segment</param>
    public PowampSpikeAnimation(PowampSpikeSegment segment)
        : base(segment, PowampSpikeAnimations.StartingAt[segment], EnemyProjectileCueRunners.Common<PowampSpikeSegment>())
    {
    }

    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<PowampSpikeSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => PowampSpikeAnimations.Segments;
}
