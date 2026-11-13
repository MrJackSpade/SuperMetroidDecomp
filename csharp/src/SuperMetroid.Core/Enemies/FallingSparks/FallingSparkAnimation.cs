using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>Falling spark's typed animation.</summary>
internal sealed class FallingSparkAnimation(FallingSparkSegment segment)
    : EnemyProjectileFrameAnimation<FallingSparkSegment, EnemyProjectileCommonCue>(FallingSparkAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<FallingSparkSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => FallingSparkAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<FallingSparkSegment, Animation<FallingSparkSegment>> StartingAt => FallingSparkAnimations.StartingAt;
}
