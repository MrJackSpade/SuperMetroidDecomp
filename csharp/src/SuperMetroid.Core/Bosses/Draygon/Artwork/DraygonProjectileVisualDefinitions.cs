using SuperMetroid.Core.Enemies.Projectiles;


namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// The presentation boundary of Draygon's projectiles (#641): the projectile frame every typed
/// frame shows.
/// </summary>
internal static class DraygonProjectileVisualDefinitions
{
    /// <summary>Distinct frames: six goop, two goop-burst, sixteen bloom and three flight frames.</summary>
    internal const int ProjectileFrameCount = 27;

    /// <summary>Every frame in installed-artwork order: goop, burst, bloom, then flight.</summary>
    internal static IReadOnlyList<DraygonProjectileFrame> ProjectileFrames { get; } =
    [
        .. Frames(DraygonProjectileFrameKind.Goop, DraygonProjectileAnimations.GoopPhaseCount),
        .. Frames(DraygonProjectileFrameKind.GoopShot, DraygonProjectileAnimations.GoopShotPhaseCount),
        .. Frames(DraygonProjectileFrameKind.WallTurretBloom, DraygonProjectileAnimations.BloomPhaseCount),
        .. Frames(DraygonProjectileFrameKind.WallTurretFlight, DraygonProjectileAnimations.FlightPhaseCount),
    ];

    /// <summary>The <paramref name="count"/> frames of <paramref name="kind"/>.</summary>
    private static IEnumerable<DraygonProjectileFrame> Frames(DraygonProjectileFrameKind kind, int count) =>
        Enumerable.Range(0, count).Select(phase => new DraygonProjectileFrame(kind, phase));

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<DraygonProjectileFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<DraygonProjectileFrame, EnemyProjectileFrameId>
        {
            [new(DraygonProjectileFrameKind.Goop, 0)] = EnemyProjectileFrameId.DraygonFrame00,
            [new(DraygonProjectileFrameKind.Goop, 1)] = EnemyProjectileFrameId.DraygonFrame01,
            [new(DraygonProjectileFrameKind.Goop, 2)] = EnemyProjectileFrameId.DraygonFrame02,
            [new(DraygonProjectileFrameKind.Goop, 3)] = EnemyProjectileFrameId.DraygonFrame03,
            [new(DraygonProjectileFrameKind.Goop, 4)] = EnemyProjectileFrameId.DraygonFrame04,
            [new(DraygonProjectileFrameKind.Goop, 5)] = EnemyProjectileFrameId.DraygonFrame05,
            [new(DraygonProjectileFrameKind.GoopShot, 0)] = EnemyProjectileFrameId.DraygonFrame06,
            [new(DraygonProjectileFrameKind.GoopShot, 1)] = EnemyProjectileFrameId.DraygonFrame07,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 0)] = EnemyProjectileFrameId.DraygonFrame08,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 1)] = EnemyProjectileFrameId.DraygonFrame09,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 2)] = EnemyProjectileFrameId.DraygonFrame10,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 3)] = EnemyProjectileFrameId.DraygonFrame11,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 4)] = EnemyProjectileFrameId.DraygonFrame12,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 5)] = EnemyProjectileFrameId.DraygonFrame13,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 6)] = EnemyProjectileFrameId.DraygonFrame14,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 7)] = EnemyProjectileFrameId.DraygonFrame15,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 8)] = EnemyProjectileFrameId.DraygonFrame16,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 9)] = EnemyProjectileFrameId.DraygonFrame17,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 10)] = EnemyProjectileFrameId.DraygonFrame18,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 11)] = EnemyProjectileFrameId.DraygonFrame19,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 12)] = EnemyProjectileFrameId.DraygonFrame20,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 13)] = EnemyProjectileFrameId.DraygonFrame21,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 14)] = EnemyProjectileFrameId.DraygonFrame22,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 15)] = EnemyProjectileFrameId.DraygonFrame23,
            [new(DraygonProjectileFrameKind.WallTurretFlight, 0)] = EnemyProjectileFrameId.DraygonFrame24,
            [new(DraygonProjectileFrameKind.WallTurretFlight, 1)] = EnemyProjectileFrameId.DraygonFrame25,
            [new(DraygonProjectileFrameKind.WallTurretFlight, 2)] = EnemyProjectileFrameId.DraygonFrame26,
        };
}
