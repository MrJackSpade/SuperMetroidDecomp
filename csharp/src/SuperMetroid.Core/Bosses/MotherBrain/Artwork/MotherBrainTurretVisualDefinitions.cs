using SuperMetroid.Core.Enemies.Projectiles;


namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The presentation boundary of Mother Brain's turrets (#641): the projectile frame every typed
/// frame shows.
/// </summary>
internal static class MotherBrainTurretVisualDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<MotherBrainTurretFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<MotherBrainTurretFrame, EnemyProjectileFrameId>
        {
            [new(MotherBrainTurretSegment.TurretPose, 0)] = EnemyProjectileFrameId.MotherBrainTurretPoseFirst,
            [new(MotherBrainTurretSegment.TurretPose, 1)] = EnemyProjectileFrameId.MotherBrainTurretPoseSecond,
            [new(MotherBrainTurretSegment.TurretPose, 2)] = EnemyProjectileFrameId.MotherBrainTurretPoseThird,
            [new(MotherBrainTurretSegment.TurretPose, 3)] = EnemyProjectileFrameId.MotherBrainTurretPoseFourth,
            [new(MotherBrainTurretSegment.TurretPose, 4)] = EnemyProjectileFrameId.MotherBrainTurretPoseFifth,
            [new(MotherBrainTurretSegment.TurretPose, 5)] = EnemyProjectileFrameId.MotherBrainTurretPoseSixth,
            [new(MotherBrainTurretSegment.TurretPose, 6)] = EnemyProjectileFrameId.MotherBrainTurretPoseSeventh,
            [new(MotherBrainTurretSegment.TurretPose, 7)] = EnemyProjectileFrameId.MotherBrainTurretPoseEighth,
            [new(MotherBrainTurretSegment.BulletPose, 0)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseFirst,
            [new(MotherBrainTurretSegment.BulletPose, 1)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseSecond,
            [new(MotherBrainTurretSegment.BulletPose, 2)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseThird,
            [new(MotherBrainTurretSegment.BulletPose, 3)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseFourth,
            [new(MotherBrainTurretSegment.BulletPose, 4)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseFifth,
            [new(MotherBrainTurretSegment.BulletPose, 5)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseSixth,
            [new(MotherBrainTurretSegment.BulletPose, 6)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseSeventh,
            [new(MotherBrainTurretSegment.BulletPose, 7)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseEighth,
            [new(MotherBrainTurretSegment.BulletImpact, 0)] = EnemyProjectileFrameId.MotherBrainTurretBulletImpactFirst,
            [new(MotherBrainTurretSegment.BulletImpact, 1)] = EnemyProjectileFrameId.MotherBrainTurretBulletImpactSecond,
            [new(MotherBrainTurretSegment.BulletImpact, 2)] = EnemyProjectileFrameId.MotherBrainTurretBulletImpactThird,
            [new(MotherBrainTurretSegment.BulletImpact, 3)] = EnemyProjectileFrameId.MotherBrainTurretBulletImpactFourth,
            [new(MotherBrainTurretSegment.BulletImpact, 4)] = EnemyProjectileFrameId.MotherBrainTurretBulletImpactFifth,
        };
}
