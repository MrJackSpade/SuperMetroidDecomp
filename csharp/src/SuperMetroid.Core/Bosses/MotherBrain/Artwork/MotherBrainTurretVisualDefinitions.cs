using SuperMetroid.Core.Enemies.Projectiles;


namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The presentation boundary of Mother Brain's turrets (#641): the projectile frame every typed
/// frame shows.
/// </summary>
internal static class MotherBrainTurretVisualDefinitions
{

    /// <summary>The projectile frame each frame of the family shows: each turret and bullet pose by the direction it faces, then the smoke's phases.</summary>
    internal static IReadOnlyDictionary<MotherBrainTurretFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<MotherBrainTurretFrame, EnemyProjectileFrameId>
        {
            [new(MotherBrainTurretSegment.TurretPose, MotherBrainTurretDirection.Left, 0)] = EnemyProjectileFrameId.MotherBrainTurretPoseFirst,
            [new(MotherBrainTurretSegment.TurretPose, MotherBrainTurretDirection.DownLeft, 0)] = EnemyProjectileFrameId.MotherBrainTurretPoseSecond,
            [new(MotherBrainTurretSegment.TurretPose, MotherBrainTurretDirection.Down, 0)] = EnemyProjectileFrameId.MotherBrainTurretPoseThird,
            [new(MotherBrainTurretSegment.TurretPose, MotherBrainTurretDirection.DownRight, 0)] = EnemyProjectileFrameId.MotherBrainTurretPoseFourth,
            [new(MotherBrainTurretSegment.TurretPose, MotherBrainTurretDirection.Right, 0)] = EnemyProjectileFrameId.MotherBrainTurretPoseFifth,
            [new(MotherBrainTurretSegment.TurretPose, MotherBrainTurretDirection.UpRight, 0)] = EnemyProjectileFrameId.MotherBrainTurretPoseSixth,
            [new(MotherBrainTurretSegment.TurretPose, MotherBrainTurretDirection.Up, 0)] = EnemyProjectileFrameId.MotherBrainTurretPoseSeventh,
            [new(MotherBrainTurretSegment.TurretPose, MotherBrainTurretDirection.UpLeft, 0)] = EnemyProjectileFrameId.MotherBrainTurretPoseEighth,
            [new(MotherBrainTurretSegment.BulletPose, MotherBrainTurretDirection.Left, 0)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseFirst,
            [new(MotherBrainTurretSegment.BulletPose, MotherBrainTurretDirection.DownLeft, 0)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseSecond,
            [new(MotherBrainTurretSegment.BulletPose, MotherBrainTurretDirection.Down, 0)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseThird,
            [new(MotherBrainTurretSegment.BulletPose, MotherBrainTurretDirection.DownRight, 0)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseFourth,
            [new(MotherBrainTurretSegment.BulletPose, MotherBrainTurretDirection.Right, 0)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseFifth,
            [new(MotherBrainTurretSegment.BulletPose, MotherBrainTurretDirection.UpRight, 0)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseSixth,
            [new(MotherBrainTurretSegment.BulletPose, MotherBrainTurretDirection.Up, 0)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseSeventh,
            [new(MotherBrainTurretSegment.BulletPose, MotherBrainTurretDirection.UpLeft, 0)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseEighth,
            [new(MotherBrainTurretSegment.BulletImpact, MotherBrainTurretDirection.Left, 0)] = EnemyProjectileFrameId.MotherBrainTurretBulletImpactFirst,
            [new(MotherBrainTurretSegment.BulletImpact, MotherBrainTurretDirection.Left, 1)] = EnemyProjectileFrameId.MotherBrainTurretBulletImpactSecond,
            [new(MotherBrainTurretSegment.BulletImpact, MotherBrainTurretDirection.Left, 2)] = EnemyProjectileFrameId.MotherBrainTurretBulletImpactThird,
            [new(MotherBrainTurretSegment.BulletImpact, MotherBrainTurretDirection.Left, 3)] = EnemyProjectileFrameId.MotherBrainTurretBulletImpactFourth,
            [new(MotherBrainTurretSegment.BulletImpact, MotherBrainTurretDirection.Left, 4)] = EnemyProjectileFrameId.MotherBrainTurretBulletImpactFifth,
        };
}
