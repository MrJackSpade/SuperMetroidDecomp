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
        new(33), new(38), new(39), new(62), new(63), new(64), new(65),
        new(41), new(42), new(43), new(44), new(45), new(46),
        new(47), new(48), new(49), new(50),
        new(76), new(77), new(78), new(79), new(80), new(81),
        new(40),
    ];
}
