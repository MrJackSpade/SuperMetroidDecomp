using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Alcoon.Artwork;

/// <summary>
/// The presentation boundary of Alcoon's fireball programs (#641): the projectile frame each
/// <see cref="AlcoonFireballFrame"/> pose shows.
/// </summary>
internal static class AlcoonFireballFrameDefinitions
{
    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<AlcoonFireballFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<AlcoonFireballFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.AlcoonFireballFrame00,
            [new(1)] = EnemyProjectileFrameId.AlcoonFireballFrame01,
            [new(2)] = EnemyProjectileFrameId.AlcoonFireballFrame02,
            [new(3)] = EnemyProjectileFrameId.AlcoonFireballFrame03,
        };
}
