using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A turret's, bullet's or impact smoke's typed animation.</summary>
internal sealed partial class MotherBrainTurretAnimation
    : EnemyProjectileFrameAnimation<MotherBrainTurretPose, MotherBrainTurretCue>
{
    /// <summary>Creates a <see cref="MotherBrainTurretAnimation"/>.</summary>
    /// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
    /// <param name="pose">The turret's or bullet's pose facing its direction, or the impact smoke.</param>
    public MotherBrainTurretAnimation(IEnemyProjectileCues<MotherBrainTurretCue> owner, MotherBrainTurretPose pose)
        : base(pose, MotherBrainTurretAnimations.StartingAt[pose], EnemyProjectileCueRunner<MotherBrainTurretPose, MotherBrainTurretCue>.By(owner))
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainTurretPose, EnemyProjectileFrameId, MotherBrainTurretCue> Segments =>
        MotherBrainTurretAnimations.Segments;
}
