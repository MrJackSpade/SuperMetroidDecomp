using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Magdollite lava's typed animation.</summary>
internal sealed partial class MagdolliteLavaAnimation
    : EnemyProjectileFrameAnimation<MagdolliteLavaSegment, MagdolliteLavaCue>
{
    /// <summary>Creates a <see cref="MagdolliteLavaAnimation"/>.</summary>
    /// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
    /// <param name="segment">Where the animation starts.</param>
    public MagdolliteLavaAnimation(IEnemyProjectileCues<MagdolliteLavaCue> owner, MagdolliteLavaSegment segment)
        : base(segment, MagdolliteLavaAnimations.StartingAt[segment], EnemyProjectileCueRunner<MagdolliteLavaSegment, MagdolliteLavaCue>.By(owner))
    {
    }

    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<MagdolliteLavaSegment, EnemyProjectileFrameId, MagdolliteLavaCue> Segments => MagdolliteLavaAnimations.Segments;
}
