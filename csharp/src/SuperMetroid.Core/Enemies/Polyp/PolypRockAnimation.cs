using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>Polyp lava rock's typed animation.</summary>
internal sealed class PolypRockAnimation(PolypRockSegment program)
    : EnemyProjectileFrameAnimation<PolypRockSegment, EnemyProjectileCommonCue>(PolypRockAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<PolypRockSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => PolypRockAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<PolypRockSegment, Animation<PolypRockSegment>> StartingAt => PolypRockAnimations.StartingAt;
}
