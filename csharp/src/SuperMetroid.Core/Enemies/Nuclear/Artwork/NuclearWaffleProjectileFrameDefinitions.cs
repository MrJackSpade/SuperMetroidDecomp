using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Nuclear.Artwork;

/// <summary>
/// The presentation boundary of the Nuclear Waffle's body programs (#641): the projectile frame each
/// <see cref="NuclearWaffleProjectileFrame"/> pose shows.
/// </summary>
internal static class NuclearWaffleProjectileFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 12;

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<NuclearWaffleProjectileFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<NuclearWaffleProjectileFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.NuclearWaffleFrame00,
            [new(1)] = EnemyProjectileFrameId.NuclearWaffleFrame01,
            [new(2)] = EnemyProjectileFrameId.NuclearWaffleFrame02,
            [new(3)] = EnemyProjectileFrameId.NuclearWaffleFrame03,
            [new(4)] = EnemyProjectileFrameId.NuclearWaffleFrame04,
            [new(5)] = EnemyProjectileFrameId.NuclearWaffleFrame05,
            [new(6)] = EnemyProjectileFrameId.NuclearWaffleFrame06,
            [new(7)] = EnemyProjectileFrameId.NuclearWaffleFrame07,
            [new(8)] = EnemyProjectileFrameId.NuclearWaffleFrame08,
            [new(9)] = EnemyProjectileFrameId.NuclearWaffleFrame09,
            [new(10)] = EnemyProjectileFrameId.NuclearWaffleFrame10,
            [new(11)] = EnemyProjectileFrameId.NuclearWaffleFrame11,
        };
}
