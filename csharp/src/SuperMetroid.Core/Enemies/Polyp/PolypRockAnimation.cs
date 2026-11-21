using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>Polyp lava rock's typed animation.</summary>
internal sealed partial class PolypRockAnimation
    : EnemyProjectileFrameAnimation<PolypRockSegment, EnemyProjectileCommonCue>
{
    /// <summary>Creates a <see cref="PolypRockAnimation"/>.</summary>
    /// <param name="segment">segment</param>
    public PolypRockAnimation(PolypRockSegment segment)
        : base(segment, PolypRockAnimations.StartingAt[segment], EnemyProjectileCueRunners.Common<PolypRockSegment>())
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<PolypRockSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => PolypRockAnimations.Segments;
}
