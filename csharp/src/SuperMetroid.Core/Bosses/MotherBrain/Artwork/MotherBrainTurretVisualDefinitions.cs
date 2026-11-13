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
            [new(MotherBrainTurretProgram.TurretPose, 0)] = EnemyProjectileFrameId.MotherBrainTurretPoseFirst,
            [new(MotherBrainTurretProgram.TurretPose, 1)] = EnemyProjectileFrameId.MotherBrainTurretPoseSecond,
            [new(MotherBrainTurretProgram.TurretPose, 2)] = EnemyProjectileFrameId.MotherBrainTurretPoseThird,
            [new(MotherBrainTurretProgram.TurretPose, 3)] = EnemyProjectileFrameId.MotherBrainTurretPoseFourth,
            [new(MotherBrainTurretProgram.TurretPose, 4)] = EnemyProjectileFrameId.MotherBrainTurretPoseFifth,
            [new(MotherBrainTurretProgram.TurretPose, 5)] = EnemyProjectileFrameId.MotherBrainTurretPoseSixth,
            [new(MotherBrainTurretProgram.TurretPose, 6)] = EnemyProjectileFrameId.MotherBrainTurretPoseSeventh,
            [new(MotherBrainTurretProgram.TurretPose, 7)] = EnemyProjectileFrameId.MotherBrainTurretPoseEighth,
            [new(MotherBrainTurretProgram.BulletPose, 0)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseFirst,
            [new(MotherBrainTurretProgram.BulletPose, 1)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseSecond,
            [new(MotherBrainTurretProgram.BulletPose, 2)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseThird,
            [new(MotherBrainTurretProgram.BulletPose, 3)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseFourth,
            [new(MotherBrainTurretProgram.BulletPose, 4)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseFifth,
            [new(MotherBrainTurretProgram.BulletPose, 5)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseSixth,
            [new(MotherBrainTurretProgram.BulletPose, 6)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseSeventh,
            [new(MotherBrainTurretProgram.BulletPose, 7)] = EnemyProjectileFrameId.MotherBrainTurretBulletPoseEighth,
            [new(MotherBrainTurretProgram.BulletImpact, 0)] = EnemyProjectileFrameId.MotherBrainTurretBulletImpactFirst,
            [new(MotherBrainTurretProgram.BulletImpact, 1)] = EnemyProjectileFrameId.MotherBrainTurretBulletImpactSecond,
            [new(MotherBrainTurretProgram.BulletImpact, 2)] = EnemyProjectileFrameId.MotherBrainTurretBulletImpactThird,
            [new(MotherBrainTurretProgram.BulletImpact, 3)] = EnemyProjectileFrameId.MotherBrainTurretBulletImpactFourth,
            [new(MotherBrainTurretProgram.BulletImpact, 4)] = EnemyProjectileFrameId.MotherBrainTurretBulletImpactFifth,
        };
}
