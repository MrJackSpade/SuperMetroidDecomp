using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Polyp.Artwork;

/// <summary>
/// The presentation boundary of the Polyp's lava rock programs (#641): the projectile frame each
/// <see cref="PolypRockFrame"/> pose shows.
/// </summary>
internal static class PolypRockFrameDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<PolypRockFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<PolypRockFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.PolypRock00,
        };
}
