using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Magdollite lava's typed animation.</summary>
internal sealed class MagdolliteLavaAnimation(MagdolliteLavaProgram program)
    : EnemyProjectileTypedAnimation<MagdolliteLavaProgram, MagdolliteLavaFrame, MagdolliteLavaCue>(MagdolliteLavaAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MagdolliteLavaProgram, MagdolliteLavaFrame, MagdolliteLavaCue> Segments => MagdolliteLavaAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MagdolliteLavaProgram, Animation<MagdolliteLavaProgram>> StartingAt => MagdolliteLavaAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(MagdolliteLavaFrame frame) =>
        MagdolliteLavaFrameDefinitions.ShownProjectileFrames[frame];
}
