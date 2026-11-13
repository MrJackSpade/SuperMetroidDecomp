using SuperMetroid.Core.Enemies.Projectiles;


namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// The presentation boundary of Draygon's projectiles (#641): the projectile frame every typed
/// frame shows.
/// </summary>
internal static class DraygonProjectileVisualDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<DraygonProjectileFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<DraygonProjectileFrame, EnemyProjectileFrameId>
        {
            [new(DraygonProjectileFrameKind.Goop, 0)] = EnemyProjectileFrameId.DraygonGoopFirst,
            [new(DraygonProjectileFrameKind.Goop, 1)] = EnemyProjectileFrameId.DraygonGoopSecond,
            [new(DraygonProjectileFrameKind.Goop, 2)] = EnemyProjectileFrameId.DraygonGoopThird,
            [new(DraygonProjectileFrameKind.Goop, 3)] = EnemyProjectileFrameId.DraygonGoopFourth,
            [new(DraygonProjectileFrameKind.Goop, 4)] = EnemyProjectileFrameId.DraygonGoopFifth,
            [new(DraygonProjectileFrameKind.Goop, 5)] = EnemyProjectileFrameId.DraygonGoopSixth,
            [new(DraygonProjectileFrameKind.GoopShot, 0)] = EnemyProjectileFrameId.DraygonGoopShotFirst,
            [new(DraygonProjectileFrameKind.GoopShot, 1)] = EnemyProjectileFrameId.DraygonGoopShotSecond,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 0)] = EnemyProjectileFrameId.DraygonWallTurretBloomFirst,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 1)] = EnemyProjectileFrameId.DraygonWallTurretBloomSecond,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 2)] = EnemyProjectileFrameId.DraygonWallTurretBloomThird,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 3)] = EnemyProjectileFrameId.DraygonWallTurretBloomFourth,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 4)] = EnemyProjectileFrameId.DraygonWallTurretBloomFifth,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 5)] = EnemyProjectileFrameId.DraygonWallTurretBloomSixth,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 6)] = EnemyProjectileFrameId.DraygonWallTurretBloomSeventh,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 7)] = EnemyProjectileFrameId.DraygonWallTurretBloomEighth,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 8)] = EnemyProjectileFrameId.DraygonWallTurretBloomNinth,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 9)] = EnemyProjectileFrameId.DraygonWallTurretBloomTenth,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 10)] = EnemyProjectileFrameId.DraygonWallTurretBloomEleventh,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 11)] = EnemyProjectileFrameId.DraygonWallTurretBloomTwelfth,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 12)] = EnemyProjectileFrameId.DraygonWallTurretBloomThirteenth,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 13)] = EnemyProjectileFrameId.DraygonWallTurretBloomFourteenth,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 14)] = EnemyProjectileFrameId.DraygonWallTurretBloomFifteenth,
            [new(DraygonProjectileFrameKind.WallTurretBloom, 15)] = EnemyProjectileFrameId.DraygonWallTurretBloomSixteenth,
            [new(DraygonProjectileFrameKind.WallTurretFlight, 0)] = EnemyProjectileFrameId.DraygonWallTurretFlightFirst,
            [new(DraygonProjectileFrameKind.WallTurretFlight, 1)] = EnemyProjectileFrameId.DraygonWallTurretFlightSecond,
            [new(DraygonProjectileFrameKind.WallTurretFlight, 2)] = EnemyProjectileFrameId.DraygonWallTurretFlightThird,
        };
}
