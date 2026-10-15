using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>A Phantoon flame's typed animation.</summary>
internal sealed class PhantoonFlameAnimation(PhantoonFlameProgram program)
    : EnemyProjectileProgramAnimation<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue> Programs =>
        PhantoonFlameAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFramePresentation Present(PhantoonFlameFrame frame) =>
        EnemyProjectileFramePresentation.Installed(PhantoonFlameVisualDefinitions.PresentationKey(frame));
}