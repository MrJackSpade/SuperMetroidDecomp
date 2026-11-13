namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The presentation boundary of Kraid's rocks (#641): the projectile frame every typed frame
/// shows.
/// </summary>
internal static class KraidRockVisualDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<KraidRockFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<KraidRockFrame, EnemyProjectileFrameId>
        {
            [new(KraidRockProgram.SharedPose, 0)] = EnemyProjectileFrameId.KraidRockSharedPoseFirst,
            [new(KraidRockProgram.SharedPose, 4)] = EnemyProjectileFrameId.KraidRockSharedPoseFifth,
            [new(KraidRockProgram.RisingRightPose, 0)] = EnemyProjectileFrameId.KraidRockRisingRightPose,
            [new(KraidRockProgram.SpitRockShot, 0)] = EnemyProjectileFrameId.KraidRockSharedPoseFifth,
            [new(KraidRockProgram.SpitRockShot, 1)] = EnemyProjectileFrameId.KraidRockSpitRockShotSecond,
            [new(KraidRockProgram.SpitRockShot, 2)] = EnemyProjectileFrameId.KraidRockSpitRockShotThird,
            [new(KraidRockProgram.SpitRockShot, 3)] = EnemyProjectileFrameId.KraidRockSpitRockShotFourth,
            [new(KraidRockProgram.SpitRockShot, 4)] = EnemyProjectileFrameId.KraidRockSpitRockShotFifth,
        };
}
