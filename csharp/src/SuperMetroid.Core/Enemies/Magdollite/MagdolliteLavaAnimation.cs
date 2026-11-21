using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Magdollite lava's typed animation.</summary>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
/// <param name="segment">Where the animation starts.</param>
internal sealed class MagdolliteLavaAnimation(IEnemyProjectileCues<MagdolliteLavaCue> owner, MagdolliteLavaSegment segment)
    : EnemyProjectileFrameAnimation<MagdolliteLavaSegment, MagdolliteLavaCue>(segment, MagdolliteLavaAnimations.StartingAt[segment], EnemyProjectileCueRunner<MagdolliteLavaSegment, MagdolliteLavaCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MagdolliteLavaSegment, EnemyProjectileFrameId, MagdolliteLavaCue> Segments => MagdolliteLavaAnimations.Segments;
}
