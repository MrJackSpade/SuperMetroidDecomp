using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Rooms.Plms.Artwork;

/// <summary>
/// The presentation boundary of the downward shot gate's programs (#641): the projectile frame each
/// <see cref="DownwardGateProjectileFrame"/> pose shows.
/// </summary>
internal static class DownwardGateProjectileFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 9;

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<DownwardGateProjectileFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<DownwardGateProjectileFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.DownwardGateFrame00,
            [new(1)] = EnemyProjectileFrameId.DownwardGateFrame01,
            [new(2)] = EnemyProjectileFrameId.DownwardGateFrame02,
            [new(3)] = EnemyProjectileFrameId.DownwardGateFrame03,
            [new(4)] = EnemyProjectileFrameId.DownwardGateFrame04,
            [new(5)] = EnemyProjectileFrameId.DownwardGateFrame05,
            [new(6)] = EnemyProjectileFrameId.DownwardGateFrame06,
            [new(7)] = EnemyProjectileFrameId.DownwardGateFrame07,
            [new(8)] = EnemyProjectileFrameId.DownwardGateFrame08,
        };
}
