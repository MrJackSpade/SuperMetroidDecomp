using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Shaktool.Artwork;

/// <summary>
/// The presentation boundary of Shaktool's attack circles programs (#641): the projectile frame each
/// <see cref="ShaktoolProjectileFrame"/> pose shows.
/// </summary>
internal static class ShaktoolProjectileFrameDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<ShaktoolProjectileFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<ShaktoolProjectileFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.ShaktoolFrame00,
            [new(1)] = EnemyProjectileFrameId.ShaktoolFrame01,
            [new(2)] = EnemyProjectileFrameId.ShaktoolFrame02,
            [new(3)] = EnemyProjectileFrameId.ShaktoolFrame03,
            [new(4)] = EnemyProjectileFrameId.ShaktoolFrame04,
            [new(5)] = EnemyProjectileFrameId.ShaktoolFrame05,
            [new(6)] = EnemyProjectileFrameId.ShaktoolFrame06,
            [new(7)] = EnemyProjectileFrameId.ShaktoolFrame07,
        };
}
