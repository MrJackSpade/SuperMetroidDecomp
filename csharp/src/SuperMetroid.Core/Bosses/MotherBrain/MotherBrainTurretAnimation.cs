using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A turret's, bullet's or impact smoke's typed animation.</summary>
/// <param name="pose">The turret's or bullet's pose facing its direction, or the impact smoke.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class MotherBrainTurretAnimation(MotherBrainTurretPose pose, IEnemyProjectileCues<MotherBrainTurretCue> cues)
    : EnemyProjectileFrameAnimation<MotherBrainTurretPose, MotherBrainTurretCue>(MotherBrainTurretAnimations.StartingAt[pose], cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainTurretPose, EnemyProjectileFrameId, MotherBrainTurretCue> Segments =>
        MotherBrainTurretAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MotherBrainTurretPose, Animation<MotherBrainTurretPose>> StartingAt =>
        MotherBrainTurretAnimations.StartingAt;
}
