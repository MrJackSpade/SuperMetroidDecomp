using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Six named visual selections made by the five Ceres flight instruction streams. The vortex stream alternates
/// two drawings; the other streams each select one drawing. Catalog order and published asset names remain stable.</summary>
public static class CeresFlightSpriteDefinitions
{
    /// <summary>The stars, large asteroid, station under attack, small asteroid and the two vortex drawings.</summary>
    public static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("stars", CinematicSpritePose.FlightStars, 25),
        new("large-asteroid", CinematicSpritePose.LargeAsteroid, 19),
        new("station-under-attack", CinematicSpritePose.StationUnderAttack, 41),
        new("small-asteroid", CinematicSpritePose.SmallAsteroid, 16),
        new("vortex-even", CinematicSpritePose.VortexEven, 36),
        new("vortex-odd", CinematicSpritePose.VortexOdd, 33),
    ];
}
