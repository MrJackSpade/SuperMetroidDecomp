using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Magdollite lava's typed animation.</summary>
internal sealed class MagdolliteLavaAnimation(MagdolliteLavaProgram program)
    : EnemyProjectileProgramAnimation<MagdolliteLavaProgram, MagdolliteLavaFrame, MagdolliteLavaCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<MagdolliteLavaProgram, MagdolliteLavaFrame, MagdolliteLavaCue> Programs => MagdolliteLavaAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(MagdolliteLavaFrame frame) =>
        MagdolliteLavaFrameDefinitions.ShownProjectileFrames[frame];
}
