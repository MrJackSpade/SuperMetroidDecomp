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
            [new(KraidRockSegment.SharedPose, 0)] = EnemyProjectileFrameId.KraidRockSharedPoseFirst,
            [new(KraidRockSegment.SharedPose, 4)] = EnemyProjectileFrameId.KraidRockSharedPoseFifth,
            [new(KraidRockSegment.RisingRightPose, 0)] = EnemyProjectileFrameId.KraidRockRisingRightPose,
            [new(KraidRockSegment.SpitRockShot, 0)] = EnemyProjectileFrameId.KraidRockSharedPoseFifth,
            [new(KraidRockSegment.SpitRockShot, 1)] = EnemyProjectileFrameId.KraidRockSpitRockShotSecond,
            [new(KraidRockSegment.SpitRockShot, 2)] = EnemyProjectileFrameId.KraidRockSpitRockShotThird,
            [new(KraidRockSegment.SpitRockShot, 3)] = EnemyProjectileFrameId.KraidRockSpitRockShotFourth,
            [new(KraidRockSegment.SpitRockShot, 4)] = EnemyProjectileFrameId.KraidRockSpitRockShotFifth,
        };
}
