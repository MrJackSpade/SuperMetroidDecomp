using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>Six named visual selections made by the five Ceres flight instruction streams. The vortex stream alternates
/// two drawings; the other streams each select one drawing. Catalog order and published asset names remain stable.</summary>
public static class CeresFlightSpriteDefinitions
{
    /// <summary>The stars, large asteroid, station under attack, small asteroid and the two vortex drawings.</summary>
    public static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("stars", new(66), 25),
        new("large-asteroid", new(82), 19),
        new("station-under-attack", new(34), 41),
        new("small-asteroid", new(35), 16),
        new("vortex-even", new(36), 36),
        new("vortex-odd", new(37), 33),
    ];

    /// <summary>The star field the flight background stream shows.</summary>
    internal static CinematicSpritePose Stars => Frames[0].Pose;

    /// <summary>The large asteroids of the approach, at OBJ priority zero.</summary>
    internal static CinematicSpritePose LargeAsteroids => Frames[1].Pose;

    /// <summary>The damaged station of the attack sequence.</summary>
    internal static CinematicSpritePose StationUnderAttack => Frames[2].Pose;

    /// <summary>The small asteroids beside the station.</summary>
    internal static CinematicSpritePose SmallAsteroids => Frames[3].Pose;
}
