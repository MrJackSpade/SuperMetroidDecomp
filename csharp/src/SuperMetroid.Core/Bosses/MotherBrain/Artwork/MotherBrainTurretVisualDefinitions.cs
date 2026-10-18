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
            [new(MotherBrainTurretProgram.TurretPose, 0)] = EnemyProjectileFrameId.MotherBrainTurretFrame00,
            [new(MotherBrainTurretProgram.TurretPose, 1)] = EnemyProjectileFrameId.MotherBrainTurretFrame01,
            [new(MotherBrainTurretProgram.TurretPose, 2)] = EnemyProjectileFrameId.MotherBrainTurretFrame02,
            [new(MotherBrainTurretProgram.TurretPose, 3)] = EnemyProjectileFrameId.MotherBrainTurretFrame03,
            [new(MotherBrainTurretProgram.TurretPose, 4)] = EnemyProjectileFrameId.MotherBrainTurretFrame04,
            [new(MotherBrainTurretProgram.TurretPose, 5)] = EnemyProjectileFrameId.MotherBrainTurretFrame05,
            [new(MotherBrainTurretProgram.TurretPose, 6)] = EnemyProjectileFrameId.MotherBrainTurretFrame06,
            [new(MotherBrainTurretProgram.TurretPose, 7)] = EnemyProjectileFrameId.MotherBrainTurretFrame07,
            [new(MotherBrainTurretProgram.BulletPose, 0)] = EnemyProjectileFrameId.MotherBrainTurretFrame08,
            [new(MotherBrainTurretProgram.BulletPose, 1)] = EnemyProjectileFrameId.MotherBrainTurretFrame09,
            [new(MotherBrainTurretProgram.BulletPose, 2)] = EnemyProjectileFrameId.MotherBrainTurretFrame10,
            [new(MotherBrainTurretProgram.BulletPose, 3)] = EnemyProjectileFrameId.MotherBrainTurretFrame11,
            [new(MotherBrainTurretProgram.BulletPose, 4)] = EnemyProjectileFrameId.MotherBrainTurretFrame12,
            [new(MotherBrainTurretProgram.BulletPose, 5)] = EnemyProjectileFrameId.MotherBrainTurretFrame13,
            [new(MotherBrainTurretProgram.BulletPose, 6)] = EnemyProjectileFrameId.MotherBrainTurretFrame14,
            [new(MotherBrainTurretProgram.BulletPose, 7)] = EnemyProjectileFrameId.MotherBrainTurretFrame15,
            [new(MotherBrainTurretProgram.BulletImpact, 0)] = EnemyProjectileFrameId.MotherBrainTurretFrame16,
            [new(MotherBrainTurretProgram.BulletImpact, 1)] = EnemyProjectileFrameId.MotherBrainTurretFrame17,
            [new(MotherBrainTurretProgram.BulletImpact, 2)] = EnemyProjectileFrameId.MotherBrainTurretFrame18,
            [new(MotherBrainTurretProgram.BulletImpact, 3)] = EnemyProjectileFrameId.MotherBrainTurretFrame19,
            [new(MotherBrainTurretProgram.BulletImpact, 4)] = EnemyProjectileFrameId.MotherBrainTurretFrame20,
        };
}
