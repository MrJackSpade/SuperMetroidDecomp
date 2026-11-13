using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// The presentation boundary of Ridley's fireballs and afterburns (#641): the projectile frame each
/// typed frame shows.
/// </summary>
internal static class RidleyFireballVisualDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<RidleyFireballFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<RidleyFireballFrame, EnemyProjectileFrameId>
        {
            [new(RidleyFireballFrameKind.Launch, 0)] = EnemyProjectileFrameId.CeresRidleyLaunchFirst,
            [new(RidleyFireballFrameKind.Launch, 1)] = EnemyProjectileFrameId.CeresRidleyLaunchSecond,
            [new(RidleyFireballFrameKind.Flight, 0)] = EnemyProjectileFrameId.CeresRidleyFlightFirst,
            [new(RidleyFireballFrameKind.Flight, 1)] = EnemyProjectileFrameId.CeresRidleyFlightSecond,
            [new(RidleyFireballFrameKind.Flight, 2)] = EnemyProjectileFrameId.CeresRidleyFlightThird,
            [new(RidleyFireballFrameKind.Flight, 3)] = EnemyProjectileFrameId.CeresRidleyFlightFourth,
            [new(RidleyFireballFrameKind.AfterburnFinal, 0)] = EnemyProjectileFrameId.CeresRidleyAfterburnFinalFirst,
            [new(RidleyFireballFrameKind.AfterburnFinal, 1)] = EnemyProjectileFrameId.CeresRidleyAfterburnFinalSecond,
            [new(RidleyFireballFrameKind.AfterburnFinal, 2)] = EnemyProjectileFrameId.CeresRidleyAfterburnFinalThird,
            [new(RidleyFireballFrameKind.AfterburnFinal, 3)] = EnemyProjectileFrameId.CeresRidleyAfterburnFinalFourth,
            [new(RidleyFireballFrameKind.AfterburnFinal, 4)] = EnemyProjectileFrameId.CeresRidleyAfterburnFinalFifth,
            [new(RidleyFireballFrameKind.HorizontalCenter, 0)] = EnemyProjectileFrameId.CeresRidleyHorizontalCenterFirst,
            [new(RidleyFireballFrameKind.HorizontalCenter, 1)] = EnemyProjectileFrameId.CeresRidleyHorizontalCenterSecond,
            [new(RidleyFireballFrameKind.HorizontalCenter, 2)] = EnemyProjectileFrameId.CeresRidleyHorizontalCenterThird,
            [new(RidleyFireballFrameKind.HorizontalCenter, 3)] = EnemyProjectileFrameId.CeresRidleyHorizontalCenterFourth,
            [new(RidleyFireballFrameKind.HorizontalCenter, 4)] = EnemyProjectileFrameId.CeresRidleyHorizontalCenterFifth,
            [new(RidleyFireballFrameKind.VerticalCenter, 0)] = EnemyProjectileFrameId.CeresRidleyVerticalCenterFirst,
            [new(RidleyFireballFrameKind.VerticalCenter, 1)] = EnemyProjectileFrameId.CeresRidleyVerticalCenterSecond,
            [new(RidleyFireballFrameKind.VerticalCenter, 2)] = EnemyProjectileFrameId.CeresRidleyVerticalCenterThird,
            [new(RidleyFireballFrameKind.VerticalCenter, 3)] = EnemyProjectileFrameId.CeresRidleyVerticalCenterFourth,
            [new(RidleyFireballFrameKind.VerticalCenter, 4)] = EnemyProjectileFrameId.CeresRidleyVerticalCenterFifth,
            [new(RidleyFireballFrameKind.Directional, 0)] = EnemyProjectileFrameId.CeresRidleyDirectionalFirst,
            [new(RidleyFireballFrameKind.Directional, 1)] = EnemyProjectileFrameId.CeresRidleyDirectionalSecond,
            [new(RidleyFireballFrameKind.Directional, 2)] = EnemyProjectileFrameId.CeresRidleyDirectionalThird,
            [new(RidleyFireballFrameKind.Directional, 3)] = EnemyProjectileFrameId.CeresRidleyDirectionalFourth,
            [new(RidleyFireballFrameKind.Directional, 4)] = EnemyProjectileFrameId.CeresRidleyDirectionalFifth,
        };
}
