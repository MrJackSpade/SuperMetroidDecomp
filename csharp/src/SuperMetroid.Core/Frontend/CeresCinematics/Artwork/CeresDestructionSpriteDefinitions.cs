using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>Named Ceres destruction backdrops and bounded blast-frame catalogs.</summary>
public static class CeresDestructionSpriteDefinitions
{
    /// <summary>
    /// The 23 published compositions: the seven backdrops in <see cref="CeresDestructionBackdrop"/> order, six small blasts,
    /// four large blasts, then six station blasts. Original OAM part counts describe stock geometry without restricting
    /// edited compositions.
    /// </summary>
    public static IReadOnlyList<CeresDestructionSpriteFrameDefinition> Frames { get; } =
    [
        new("station-under-attack-large-asteroid", new(33), 19, CeresDestructionBackdrop.LargeAsteroids),
        new("planet-zebes", new(38), 50, CeresDestructionBackdrop.Planet),
        new("planet-zebes-title", new(39), 11, CeresDestructionBackdrop.Title),
        new("zebes-stars-upper-left", new(62), 12, CeresDestructionBackdrop.UpperLeftStars),
        new("zebes-stars-upper-right", new(63), 6, CeresDestructionBackdrop.UpperRightStars),
        new("zebes-stars-lower-left", new(64), 4, CeresDestructionBackdrop.LowerLeftStars),
        new("zebes-stars-lower-right", new(65), 7, CeresDestructionBackdrop.LowerRightStars),
        new("small-blast-0", new(41), 1),
        new("small-blast-1", new(42), 1),
        new("small-blast-2", new(43), 4),
        new("small-blast-3", new(44), 4),
        new("small-blast-4", new(45), 4),
        new("small-blast-5", new(46), 4),
        new("large-blast-0", new(47), 1),
        new("large-blast-1", new(48), 1),
        new("large-blast-2", new(49), 1),
        new("large-blast-3", new(50), 1),
        new("station-blast-0", new(76), 4),
        new("station-blast-1", new(77), 4),
        new("station-blast-2", new(78), 4),
        new("station-blast-3", new(79), 12),
        new("station-blast-4", new(80), 8),
        new("station-blast-5", new(81), 12),
    ];

    /// <summary>The pose of single-part asteroid explosion frame <paramref name="frame"/>, 0 through 3.</summary>
    internal static CinematicSpritePose LargeFrame(int frame) =>
        (uint)frame < 4 ? Frames[13 + frame].Pose : throw new ArgumentOutOfRangeException(nameof(frame));

    /// <summary>
    /// The OAM part count of station blast <paramref name="frame"/>: frames zero through two use one part per reflected
    /// quadrant; frame four has two lobes per quadrant; frames three and five add two tips to a core per quadrant.
    /// </summary>
    internal static int StationPartCount(int frame) =>
        (uint)frame < 6 ? Frames[17 + frame].StockPartCount : throw new ArgumentOutOfRangeException(nameof(frame));

    /// <summary>The pose of station blast <paramref name="frame"/>, 0 through 5.</summary>
    internal static CinematicSpritePose StationFrame(int frame) =>
        (uint)frame < 6 ? Frames[17 + frame].Pose : throw new ArgumentOutOfRangeException(nameof(frame));

    /// <summary>The pose that shows <paramref name="backdrop"/>.</summary>
    internal static CinematicSpritePose BackdropPose(CeresDestructionBackdrop backdrop) =>
        Enum.IsDefined(backdrop) ? Frames[(int)backdrop].Pose : throw new ArgumentOutOfRangeException(nameof(backdrop));
}
