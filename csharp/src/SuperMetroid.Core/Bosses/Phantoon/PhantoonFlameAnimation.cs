using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>A Phantoon flame's typed animation.</summary>
internal sealed class PhantoonFlameAnimation(PhantoonFlameSegment segment)
    : EnemyProjectileTypedAnimation<PhantoonFlameSegment, PhantoonFlameFrame, PhantoonFlameCue>(PhantoonFlameAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<PhantoonFlameSegment, PhantoonFlameFrame, PhantoonFlameCue> Segments =>
        PhantoonFlameAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<PhantoonFlameSegment, Animation<PhantoonFlameSegment>> StartingAt =>
        PhantoonFlameAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(PhantoonFlameFrame frame) =>
        PhantoonFlameVisualDefinitions.ShownProjectileFrames[frame];
}
