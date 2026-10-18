using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Stoke.Artwork;

/// <summary>
/// The presentation boundary of the Stoke's projectile programs (#641): the projectile frame each
/// <see cref="StokeProjectileFrame"/> pose shows.
/// </summary>
internal static class StokeProjectileFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 2;

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<StokeProjectileFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<StokeProjectileFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.StokeFrame00,
            [new(1)] = EnemyProjectileFrameId.StokeFrame01,
        };
}
