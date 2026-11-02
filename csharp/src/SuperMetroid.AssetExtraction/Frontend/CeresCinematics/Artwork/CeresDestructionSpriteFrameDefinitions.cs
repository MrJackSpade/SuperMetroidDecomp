using static SuperMetroid.Core.Frontend.CeresCinematics.Artwork.CeresDestructionSpriteDefinitions;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Named Ceres destruction backdrops and bounded blast-frame catalogs the import reads and writes (#1502).</summary>
public static class CeresDestructionSpriteFrameDefinitions
{
    /// <summary>
    /// The 23 published compositions, one for each destruction pose in order: the seven backdrops in
    /// <see cref="CeresDestructionBackdrop"/> order, six small blasts, four large blasts, then six station blasts. Original
    /// sprite part counts describe stock geometry without restricting edited compositions.
    /// </summary>
    public static IReadOnlyList<CeresDestructionSpriteFrameDefinition> Frames { get; } =
    [
        new("station-under-attack-large-asteroid", Poses[0], 19, CeresDestructionBackdrop.LargeAsteroids),
        new("planet-zebes", Poses[1], 50, CeresDestructionBackdrop.Planet),
        new("planet-zebes-title", Poses[2], 11, CeresDestructionBackdrop.Title),
        new("zebes-stars-upper-left", Poses[3], 12, CeresDestructionBackdrop.UpperLeftStars),
        new("zebes-stars-upper-right", Poses[4], 6, CeresDestructionBackdrop.UpperRightStars),
        new("zebes-stars-lower-left", Poses[5], 4, CeresDestructionBackdrop.LowerLeftStars),
        new("zebes-stars-lower-right", Poses[6], 7, CeresDestructionBackdrop.LowerRightStars),
        new("small-blast-0", Poses[7], 1),
        new("small-blast-1", Poses[8], 1),
        new("small-blast-2", Poses[9], 4),
        new("small-blast-3", Poses[10], 4),
        new("small-blast-4", Poses[11], 4),
        new("small-blast-5", Poses[12], 4),
        new("large-blast-0", Poses[13], 1),
        new("large-blast-1", Poses[14], 1),
        new("large-blast-2", Poses[15], 1),
        new("large-blast-3", Poses[16], 1),
        new("station-blast-0", Poses[17], 4),
        new("station-blast-1", Poses[18], 4),
        new("station-blast-2", Poses[19], 4),
        new("station-blast-3", Poses[20], 12),
        new("station-blast-4", Poses[21], 8),
        new("station-blast-5", Poses[22], 12),
    ];
}
