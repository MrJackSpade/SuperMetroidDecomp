using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>A Phantoon flame's typed animation.</summary>
internal sealed class PhantoonFlameAnimation(PhantoonFlameProgram program)
    : EnemyProjectileTypedAnimation<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue>(PhantoonFlameAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue> Segments =>
        PhantoonFlameAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<PhantoonFlameProgram, Animation<PhantoonFlameProgram>> StartingAt =>
        PhantoonFlameAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(PhantoonFlameFrame frame) =>
        PhantoonFlameVisualDefinitions.ShownProjectileFrames[frame];
}
