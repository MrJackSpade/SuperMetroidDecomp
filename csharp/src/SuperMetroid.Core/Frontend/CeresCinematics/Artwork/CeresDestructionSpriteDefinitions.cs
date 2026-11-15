using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>The poses of the Ceres destruction's backdrops and bounded blast frames.</summary>
public static class CeresDestructionSpriteDefinitions
{
    /// <summary>
    /// The 24 poses the destruction family shows: the seven backdrops, six small blasts, four large blasts, six station
    /// blasts, then the Japanese PLANET ZEBES subtitle (#1733).
    /// </summary>
    public static IReadOnlyList<CinematicSpritePose> Poses { get; } =
    [
        CinematicSpritePose.StationUnderAttackLargeAsteroid, CinematicSpritePose.PlanetZebes, CinematicSpritePose.PlanetZebesTitle, CinematicSpritePose.ZebesStarsUpperLeft, CinematicSpritePose.ZebesStarsUpperRight, CinematicSpritePose.ZebesStarsLowerLeft, CinematicSpritePose.ZebesStarsLowerRight,
        CinematicSpritePose.SmallExplosionSpark, CinematicSpritePose.SmallExplosionFlare, CinematicSpritePose.SmallExplosionFireball, CinematicSpritePose.SmallExplosionHollowing, CinematicSpritePose.SmallExplosionRing, CinematicSpritePose.SmallExplosionFadingRing,
        CinematicSpritePose.LargeBlastFireballCluster, CinematicSpritePose.LargeBlastBreakingCluster, CinematicSpritePose.LargeBlastScatteringEmbers, CinematicSpritePose.LargeBlastFadingEmbers,
        CinematicSpritePose.StationBlastFlare, CinematicSpritePose.StationBlastFireball, CinematicSpritePose.StationBlastBursting, CinematicSpritePose.StationBlastRingedFireball, CinematicSpritePose.StationBlastRing, CinematicSpritePose.StationBlastFadingRing,
        CinematicSpritePose.ZebesJapaneseTitle,
    ];
}
