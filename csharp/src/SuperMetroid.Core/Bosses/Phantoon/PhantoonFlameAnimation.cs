using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>A Phantoon flame's typed animation.</summary>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
/// <param name="segment">Where the animation starts.</param>
internal sealed class PhantoonFlameAnimation(IEnemyProjectileCues<PhantoonFlameCue> owner, PhantoonFlameSegment segment)
    : EnemyProjectileFrameAnimation<PhantoonFlameSegment, PhantoonFlameCue>(segment, PhantoonFlameAnimations.StartingWith(segment), EnemyProjectileCueRunner<PhantoonFlameSegment, PhantoonFlameCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<PhantoonFlameSegment, EnemyProjectileFrameId, PhantoonFlameCue> Segments =>
        PhantoonFlameAnimations.Segments;
}
