using SuperMetroid.Core.Enemies.Projectiles;


namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>
/// The presentation boundary of Kraid's rocks (#641): the projectile frame every typed frame
/// shows.
/// </summary>
internal static class KraidRockVisualDefinitions
{
    /// <summary>Distinct frames: the two still poses and five burst frames.</summary>
    internal const int ProjectileFrameCount = 7;

    /// <summary>Every frame in installed-artwork order: the two poses, then the burst.</summary>
    internal static IReadOnlyList<KraidRockFrame> ProjectileFrames { get; } =
    [
        new KraidRockFrame(KraidRockProgram.SharedPose, 0),
        new KraidRockFrame(KraidRockProgram.RisingRightPose, 0),
        .. Enumerable.Range(0, KraidRockAnimations.ShotPhaseCount).Select(phase => new KraidRockFrame(KraidRockProgram.SpitRockShot, phase)),
    ];

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<KraidRockFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<KraidRockFrame, EnemyProjectileFrameId>
        {
            [new(KraidRockProgram.SharedPose, 0)] = EnemyProjectileFrameId.KraidRockFrame00,
            [new(KraidRockProgram.SharedPose, 4)] = EnemyProjectileFrameId.KraidRockFrame02,
            [new(KraidRockProgram.RisingRightPose, 0)] = EnemyProjectileFrameId.KraidRockFrame01,
            [new(KraidRockProgram.SpitRockShot, 0)] = EnemyProjectileFrameId.KraidRockFrame02,
            [new(KraidRockProgram.SpitRockShot, 1)] = EnemyProjectileFrameId.KraidRockFrame03,
            [new(KraidRockProgram.SpitRockShot, 2)] = EnemyProjectileFrameId.KraidRockFrame04,
            [new(KraidRockProgram.SpitRockShot, 3)] = EnemyProjectileFrameId.KraidRockFrame05,
            [new(KraidRockProgram.SpitRockShot, 4)] = EnemyProjectileFrameId.KraidRockFrame06,
        };
}
