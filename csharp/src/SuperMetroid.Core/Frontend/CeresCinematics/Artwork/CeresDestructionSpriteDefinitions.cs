using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>The poses of the Ceres destruction's backdrops and bounded blast frames.</summary>
public static class CeresDestructionSpriteDefinitions
{
    /// <summary>
    /// The 23 poses the destruction family shows: the seven backdrops, six small blasts, four large blasts, then six
    /// station blasts.
    /// </summary>
    public static IReadOnlyList<CinematicSpritePose> Poses { get; } =
    [
        CinematicSpritePose.StationUnderAttackLargeAsteroid, CinematicSpritePose.PlanetZebes, CinematicSpritePose.PlanetZebesTitle, CinematicSpritePose.ZebesStarsUpperLeft, CinematicSpritePose.ZebesStarsUpperRight, CinematicSpritePose.ZebesStarsLowerLeft, CinematicSpritePose.ZebesStarsLowerRight,
        CinematicSpritePose.SmallExplosion0, CinematicSpritePose.SmallExplosion1, CinematicSpritePose.SmallExplosion2, CinematicSpritePose.SmallExplosion3, CinematicSpritePose.SmallExplosion4, CinematicSpritePose.SmallExplosion5,
        CinematicSpritePose.LargeBlast0, CinematicSpritePose.LargeBlast1, CinematicSpritePose.LargeBlast2, CinematicSpritePose.LargeBlast3,
        CinematicSpritePose.StationBlast0, CinematicSpritePose.StationBlast1, CinematicSpritePose.StationBlast2, CinematicSpritePose.StationBlast3, CinematicSpritePose.StationBlast4, CinematicSpritePose.StationBlast5,
    ];
}
