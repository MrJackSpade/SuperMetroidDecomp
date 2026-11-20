using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Magdollite lava's typed animation.</summary>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class MagdolliteLavaAnimation(MagdolliteLavaSegment segment, IEnemyProjectileCues<MagdolliteLavaCue> cues)
    : EnemyProjectileFrameAnimation<MagdolliteLavaSegment, MagdolliteLavaCue>(MagdolliteLavaAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MagdolliteLavaSegment, EnemyProjectileFrameId, MagdolliteLavaCue> Segments => MagdolliteLavaAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MagdolliteLavaSegment, Animation<MagdolliteLavaSegment>> StartingAt => MagdolliteLavaAnimations.StartingAt;
}
