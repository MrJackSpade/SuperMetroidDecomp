using SuperMetroid.Core.Enemies.Projectiles;

using K = SuperMetroid.Core.Bosses.Ridley.RidleyFireballFrameKind;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// The presentation boundary of Ridley's fireballs and afterburns (#641): the bank-$86 list entries the projectile
/// definitions name, and the projectile frame each typed frame shows.
/// </summary>
internal static class RidleyFireballVisualDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<RidleyFireballFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<RidleyFireballFrame, EnemyProjectileFrameId>
        {
            [new(RidleyFireballFrameKind.Launch, 0)] = EnemyProjectileFrameId.CeresRidleyFrame00,
            [new(RidleyFireballFrameKind.Launch, 1)] = EnemyProjectileFrameId.CeresRidleyFrame01,
            [new(RidleyFireballFrameKind.Flight, 0)] = EnemyProjectileFrameId.CeresRidleyFrame02,
            [new(RidleyFireballFrameKind.Flight, 1)] = EnemyProjectileFrameId.CeresRidleyFrame03,
            [new(RidleyFireballFrameKind.Flight, 2)] = EnemyProjectileFrameId.CeresRidleyFrame04,
            [new(RidleyFireballFrameKind.Flight, 3)] = EnemyProjectileFrameId.CeresRidleyFrame05,
            [new(RidleyFireballFrameKind.AfterburnFinal, 0)] = EnemyProjectileFrameId.CeresRidleyFrame06,
            [new(RidleyFireballFrameKind.AfterburnFinal, 1)] = EnemyProjectileFrameId.CeresRidleyFrame07,
            [new(RidleyFireballFrameKind.AfterburnFinal, 2)] = EnemyProjectileFrameId.CeresRidleyFrame08,
            [new(RidleyFireballFrameKind.AfterburnFinal, 3)] = EnemyProjectileFrameId.CeresRidleyFrame09,
            [new(RidleyFireballFrameKind.AfterburnFinal, 4)] = EnemyProjectileFrameId.CeresRidleyFrame10,
            [new(RidleyFireballFrameKind.HorizontalCenter, 0)] = EnemyProjectileFrameId.CeresRidleyFrame11,
            [new(RidleyFireballFrameKind.HorizontalCenter, 1)] = EnemyProjectileFrameId.CeresRidleyFrame12,
            [new(RidleyFireballFrameKind.HorizontalCenter, 2)] = EnemyProjectileFrameId.CeresRidleyFrame13,
            [new(RidleyFireballFrameKind.HorizontalCenter, 3)] = EnemyProjectileFrameId.CeresRidleyFrame14,
            [new(RidleyFireballFrameKind.HorizontalCenter, 4)] = EnemyProjectileFrameId.CeresRidleyFrame15,
            [new(RidleyFireballFrameKind.VerticalCenter, 0)] = EnemyProjectileFrameId.CeresRidleyFrame16,
            [new(RidleyFireballFrameKind.VerticalCenter, 1)] = EnemyProjectileFrameId.CeresRidleyFrame17,
            [new(RidleyFireballFrameKind.VerticalCenter, 2)] = EnemyProjectileFrameId.CeresRidleyFrame18,
            [new(RidleyFireballFrameKind.VerticalCenter, 3)] = EnemyProjectileFrameId.CeresRidleyFrame19,
            [new(RidleyFireballFrameKind.VerticalCenter, 4)] = EnemyProjectileFrameId.CeresRidleyFrame20,
            [new(RidleyFireballFrameKind.Directional, 0)] = EnemyProjectileFrameId.CeresRidleyFrame21,
            [new(RidleyFireballFrameKind.Directional, 1)] = EnemyProjectileFrameId.CeresRidleyFrame22,
            [new(RidleyFireballFrameKind.Directional, 2)] = EnemyProjectileFrameId.CeresRidleyFrame23,
            [new(RidleyFireballFrameKind.Directional, 3)] = EnemyProjectileFrameId.CeresRidleyFrame24,
            [new(RidleyFireballFrameKind.Directional, 4)] = EnemyProjectileFrameId.CeresRidleyFrame25,
        };
}
