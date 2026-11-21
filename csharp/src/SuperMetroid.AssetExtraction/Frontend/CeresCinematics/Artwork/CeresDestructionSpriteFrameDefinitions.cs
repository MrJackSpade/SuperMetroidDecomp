using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Named Ceres destruction backdrops and bounded blast-frame catalogs the import reads and writes (#1502).</summary>
public static class CeresDestructionSpriteFrameDefinitions
{
    /// <summary>
    /// The 24 published compositions, each naming the destruction pose it shows: the seven backdrops in their
    /// drawing order, six small blasts, four large blasts, six station blasts, then the Japanese PLANET ZEBES subtitle
    /// (#1733). Original
    /// sprite part counts describe stock geometry without restricting edited compositions.
    /// </summary>
    public static IReadOnlyList<CeresDestructionSpriteFrameDefinition> Frames { get; } =
    [
        new("station-under-attack-large-asteroid", CinematicSpritePose.StationUnderAttackLargeAsteroid, 19),
        new("planet-zebes", CinematicSpritePose.PlanetZebes, 50),
        new("planet-zebes-title", CinematicSpritePose.PlanetZebesTitle, 11),
        new("zebes-stars-upper-left", CinematicSpritePose.ZebesStarsUpperLeft, 12),
        new("zebes-stars-upper-right", CinematicSpritePose.ZebesStarsUpperRight, 6),
        new("zebes-stars-lower-left", CinematicSpritePose.ZebesStarsLowerLeft, 4),
        new("zebes-stars-lower-right", CinematicSpritePose.ZebesStarsLowerRight, 7),
        new("small-blast-0", CinematicSpritePose.SmallExplosionSpark, 1),
        new("small-blast-1", CinematicSpritePose.SmallExplosionFlare, 1),
        new("small-blast-2", CinematicSpritePose.SmallExplosionFireball, 4),
        new("small-blast-3", CinematicSpritePose.SmallExplosionHollowing, 4),
        new("small-blast-4", CinematicSpritePose.SmallExplosionRing, 4),
        new("small-blast-5", CinematicSpritePose.SmallExplosionFadingRing, 4),
        new("large-blast-0", CinematicSpritePose.LargeBlastFireballCluster, 1),
        new("large-blast-1", CinematicSpritePose.LargeBlastBreakingCluster, 1),
        new("large-blast-2", CinematicSpritePose.LargeBlastScatteringEmbers, 1),
        new("large-blast-3", CinematicSpritePose.LargeBlastFadingEmbers, 1),
        new("station-blast-0", CinematicSpritePose.StationBlastFlare, 4),
        new("station-blast-1", CinematicSpritePose.StationBlastFireball, 4),
        new("station-blast-2", CinematicSpritePose.StationBlastBursting, 4),
        new("station-blast-3", CinematicSpritePose.StationBlastRingedFireball, 12),
        new("station-blast-4", CinematicSpritePose.StationBlastRing, 8),
        new("station-blast-5", CinematicSpritePose.StationBlastFadingRing, 12),
        new("planet-zebes-subtitle", CinematicSpritePose.ZebesJapaneseTitle, 12),
    ];
}
