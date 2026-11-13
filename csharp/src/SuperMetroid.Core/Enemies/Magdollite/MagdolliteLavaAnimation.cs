using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Magdollite lava's typed animation.</summary>
internal sealed class MagdolliteLavaAnimation(MagdolliteLavaProgram program)
    : EnemyProjectileFrameAnimation<MagdolliteLavaProgram, MagdolliteLavaCue>(MagdolliteLavaAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MagdolliteLavaProgram, EnemyProjectileFrameId, MagdolliteLavaCue> Segments => MagdolliteLavaAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MagdolliteLavaProgram, Animation<MagdolliteLavaProgram>> StartingAt => MagdolliteLavaAnimations.StartingAt;
}
