using SuperMetroid.Core.Enemies.Projectiles;


namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// The presentation boundary of Crocomire's projectiles (#641): the projectile frame every typed
/// <see cref="CrocomireProjectileFrame"/> shows.
/// </summary>
internal static class CrocomireProjectileVisualDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<CrocomireProjectileFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<CrocomireProjectileFrame, EnemyProjectileFrameId>
        {
            [new(CrocomireProjectileFrameKind.Mouth, 0)] = EnemyProjectileFrameId.CrocomireMouthFirst,
            [new(CrocomireProjectileFrameKind.Mouth, 1)] = EnemyProjectileFrameId.CrocomireMouthSecond,
            [new(CrocomireProjectileFrameKind.Mouth, 2)] = EnemyProjectileFrameId.CrocomireMouthThird,
            [new(CrocomireProjectileFrameKind.Mouth, 3)] = EnemyProjectileFrameId.CrocomireMouthFourth,
            [new(CrocomireProjectileFrameKind.Mouth, 4)] = EnemyProjectileFrameId.CrocomireMouthFifth,
            [new(CrocomireProjectileFrameKind.Mouth, 5)] = EnemyProjectileFrameId.CrocomireMouthSixth,
            [new(CrocomireProjectileFrameKind.BridgeFragment, 0)] = EnemyProjectileFrameId.CrocomireBridgeFragment,
            [new(CrocomireProjectileFrameKind.SpikeWallPiece, 0)] = EnemyProjectileFrameId.CrocomireSpikeWallPiece,
            [new(CrocomireProjectileFrameKind.Shot, 0)] = EnemyProjectileFrameId.CrocomireShotFirst,
            [new(CrocomireProjectileFrameKind.Shot, 1)] = EnemyProjectileFrameId.CrocomireShotSecond,
            [new(CrocomireProjectileFrameKind.Shot, 2)] = EnemyProjectileFrameId.CrocomireShotThird,
            [new(CrocomireProjectileFrameKind.Shot, 3)] = EnemyProjectileFrameId.CrocomireShotFourth,
            [new(CrocomireProjectileFrameKind.Shot, 4)] = EnemyProjectileFrameId.CrocomireShotFifth,
        };
}
