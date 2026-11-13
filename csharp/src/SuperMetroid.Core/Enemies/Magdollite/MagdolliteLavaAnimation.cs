using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Magdollite lava's typed animation.</summary>
internal sealed class MagdolliteLavaAnimation(MagdolliteLavaSegment program)
    : EnemyProjectileFrameAnimation<MagdolliteLavaSegment, MagdolliteLavaCue>(MagdolliteLavaAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MagdolliteLavaSegment, EnemyProjectileFrameId, MagdolliteLavaCue> Segments => MagdolliteLavaAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MagdolliteLavaSegment, Animation<MagdolliteLavaSegment>> StartingAt => MagdolliteLavaAnimations.StartingAt;
}
