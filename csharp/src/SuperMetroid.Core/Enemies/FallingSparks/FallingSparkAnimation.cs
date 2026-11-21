using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>Falling spark's typed animation.</summary>
internal sealed class FallingSparkAnimation(FallingSparkSegment segment)
    : EnemyProjectileFrameAnimation<FallingSparkSegment, EnemyProjectileCommonCue>(segment, FallingSparkAnimations.StartingAt[segment], EnemyProjectileCueRunners.Common<FallingSparkSegment>())
{
    /// <inheritdoc/>
    private protected override AnimationSegments<FallingSparkSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => FallingSparkAnimations.Segments;
}
