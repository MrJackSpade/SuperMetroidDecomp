using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>A Phantoon flame's typed animation.</summary>
/// <param name="segment">The segment the animation starts at.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class PhantoonFlameAnimation(PhantoonFlameSegment segment, IEnemyProjectileCues<PhantoonFlameCue> cues)
    : EnemyProjectileFrameAnimation<PhantoonFlameSegment, PhantoonFlameCue>(PhantoonFlameAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<PhantoonFlameSegment, EnemyProjectileFrameId, PhantoonFlameCue> Segments =>
        PhantoonFlameAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<PhantoonFlameSegment, Animation<PhantoonFlameSegment>> StartingAt =>
        PhantoonFlameAnimations.StartingAt;
}
