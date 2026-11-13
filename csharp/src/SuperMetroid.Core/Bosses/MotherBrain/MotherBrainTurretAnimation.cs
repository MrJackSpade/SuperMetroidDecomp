using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A turret's, bullet's or impact smoke's typed animation.</summary>
internal sealed class MotherBrainTurretAnimation
    : EnemyProjectileTypedAnimation<MotherBrainTurretPose, MotherBrainTurretFrame, MotherBrainTurretCue>
{
    /// <summary>Plays the still pose of <paramref name="segment"/> facing <paramref name="direction"/>.</summary>
    internal MotherBrainTurretAnimation(MotherBrainTurretSegment segment, MotherBrainTurretDirection direction)
        : base(segment == MotherBrainTurretSegment.BulletImpact
            ? throw new ArgumentOutOfRangeException(nameof(segment), segment, "Only turrets and bullets have direction poses.")
            : MotherBrainTurretAnimations.StartingAt[new MotherBrainTurretPose(segment, direction)])
    {
    }

    /// <summary>Plays a bullet's impact smoke.</summary>
    internal MotherBrainTurretAnimation()
        : base(MotherBrainTurretAnimations.StartingAt[MotherBrainTurretPose.BulletImpact])
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainTurretPose, MotherBrainTurretFrame, MotherBrainTurretCue> Segments =>
        MotherBrainTurretAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MotherBrainTurretPose, Animation<MotherBrainTurretPose>> StartingAt =>
        MotherBrainTurretAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(MotherBrainTurretFrame frame) =>
        MotherBrainTurretVisualDefinitions.ShownProjectileFrames[frame];
}
