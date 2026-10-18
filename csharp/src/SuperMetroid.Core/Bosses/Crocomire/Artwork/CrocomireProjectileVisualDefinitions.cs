using SuperMetroid.Core.Enemies.Projectiles;


namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// The presentation boundary of Crocomire's projectiles (#641): the projectile frame every typed
/// <see cref="CrocomireProjectileFrame"/> shows.
/// </summary>
internal static class CrocomireProjectileVisualDefinitions
{
    /// <summary>Distinct projectile frames: six mouth, one bridge fragment, one spike piece and five shot frames.</summary>
    internal const int ProjectileFrameCount = 13;

    /// <summary>Every projectile frame in installed-artwork order: mouth, bridge, spike, then shot.</summary>
    internal static IReadOnlyList<CrocomireProjectileFrame> ProjectileFrames { get; } =
    [
        .. Enumerable.Range(0, CrocomireAnimations.MouthPhaseCount).Select(phase => new CrocomireProjectileFrame(CrocomireProjectileFrameKind.Mouth, phase)),
        new CrocomireProjectileFrame(CrocomireProjectileFrameKind.BridgeFragment, 0),
        new CrocomireProjectileFrame(CrocomireProjectileFrameKind.SpikeWallPiece, 0),
        .. Enumerable.Range(0, CrocomireAnimations.ShotPhaseCount).Select(phase => new CrocomireProjectileFrame(CrocomireProjectileFrameKind.Shot, phase)),
    ];

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<CrocomireProjectileFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<CrocomireProjectileFrame, EnemyProjectileFrameId>
        {
            [new(CrocomireProjectileFrameKind.Mouth, 0)] = EnemyProjectileFrameId.CrocomireFrame00,
            [new(CrocomireProjectileFrameKind.Mouth, 1)] = EnemyProjectileFrameId.CrocomireFrame01,
            [new(CrocomireProjectileFrameKind.Mouth, 2)] = EnemyProjectileFrameId.CrocomireFrame02,
            [new(CrocomireProjectileFrameKind.Mouth, 3)] = EnemyProjectileFrameId.CrocomireFrame03,
            [new(CrocomireProjectileFrameKind.Mouth, 4)] = EnemyProjectileFrameId.CrocomireFrame04,
            [new(CrocomireProjectileFrameKind.Mouth, 5)] = EnemyProjectileFrameId.CrocomireFrame05,
            [new(CrocomireProjectileFrameKind.BridgeFragment, 0)] = EnemyProjectileFrameId.CrocomireFrame06,
            [new(CrocomireProjectileFrameKind.BridgeFragment, 2)] = EnemyProjectileFrameId.CrocomireFrame07,
            [new(CrocomireProjectileFrameKind.SpikeWallPiece, 0)] = EnemyProjectileFrameId.CrocomireFrame07,
            [new(CrocomireProjectileFrameKind.SpikeWallPiece, 5)] = EnemyProjectileFrameId.CrocomireFrame08,
            [new(CrocomireProjectileFrameKind.Shot, 0)] = EnemyProjectileFrameId.CrocomireFrame08,
            [new(CrocomireProjectileFrameKind.Shot, 1)] = EnemyProjectileFrameId.CrocomireFrame09,
            [new(CrocomireProjectileFrameKind.Shot, 2)] = EnemyProjectileFrameId.CrocomireFrame10,
            [new(CrocomireProjectileFrameKind.Shot, 3)] = EnemyProjectileFrameId.CrocomireFrame11,
            [new(CrocomireProjectileFrameKind.Shot, 4)] = EnemyProjectileFrameId.CrocomireFrame12,
        };
}
