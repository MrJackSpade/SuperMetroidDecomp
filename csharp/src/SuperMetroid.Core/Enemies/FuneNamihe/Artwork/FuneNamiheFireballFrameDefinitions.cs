using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FuneNamihe.Artwork;

/// <summary>
/// The presentation boundary of Fune and Namihe's fireball programs (#641): the projectile frame each
/// <see cref="FuneNamiheFireballFrame"/> pose shows.
/// </summary>
internal static class FuneNamiheFireballFrameDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<FuneNamiheFireballFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<FuneNamiheFireballFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.FuneNamiheFireballFrame00,
            [new(1)] = EnemyProjectileFrameId.FuneNamiheFireballFrame01,
            [new(2)] = EnemyProjectileFrameId.FuneNamiheFireballFrame02,
            [new(3)] = EnemyProjectileFrameId.FuneNamiheFireballFrame03,
            [new(4)] = EnemyProjectileFrameId.FuneNamiheFireballFrame04,
            [new(5)] = EnemyProjectileFrameId.FuneNamiheFireballFrame05,
        };
}
