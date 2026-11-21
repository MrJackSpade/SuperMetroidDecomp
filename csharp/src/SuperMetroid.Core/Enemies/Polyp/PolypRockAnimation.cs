using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>Polyp lava rock's typed animation.</summary>
internal sealed class PolypRockAnimation(PolypRockSegment segment)
    : EnemyProjectileFrameAnimation<PolypRockSegment, EnemyProjectileCommonCue>(segment, PolypRockAnimations.StartingAt[segment], EnemyProjectileCueRunners.Common<PolypRockSegment>())
{
    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<PolypRockSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => PolypRockAnimations.Segments;
}
