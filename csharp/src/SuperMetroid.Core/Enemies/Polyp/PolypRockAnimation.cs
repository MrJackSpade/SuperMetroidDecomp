using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>Polyp lava rock's typed animation.</summary>
internal sealed class PolypRockAnimation(PolypRockSegment segment)
    : EnemyProjectileFrameAnimation<PolypRockSegment, EnemyProjectileCommonCue>(PolypRockAnimations.StartingWith(segment), EnemyProjectileCueRunners.Common<PolypRockSegment>())
{
    /// <inheritdoc/>
    private protected override AnimationSegments<PolypRockSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => PolypRockAnimations.Segments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<PolypRockSegment, Animation<PolypRockSegment>>> StartingAt => PolypRockAnimations.StartingAt;
}
