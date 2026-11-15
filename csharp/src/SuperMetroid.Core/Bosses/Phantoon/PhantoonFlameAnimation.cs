using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>A Phantoon flame's typed animation.</summary>
internal sealed class PhantoonFlameAnimation(PhantoonFlameSegment segment)
    : EnemyProjectileFrameAnimation<PhantoonFlameSegment, PhantoonFlameCue>(PhantoonFlameAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<PhantoonFlameSegment, EnemyProjectileFrameId, PhantoonFlameCue> Segments =>
        PhantoonFlameAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<PhantoonFlameSegment, Animation<PhantoonFlameSegment>> StartingAt =>
        PhantoonFlameAnimations.StartingAt;
}
