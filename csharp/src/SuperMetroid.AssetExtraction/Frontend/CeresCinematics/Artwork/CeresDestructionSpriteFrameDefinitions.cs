using static SuperMetroid.Core.Frontend.CeresCinematics.Artwork.CeresDestructionSpriteDefinitions;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Named Ceres destruction backdrops and bounded blast-frame catalogs the import reads and writes (#1502).</summary>
public static class CeresDestructionSpriteFrameDefinitions
{
    /// <summary>
    /// The 24 published compositions, one for each destruction pose in order: the seven backdrops in their
    /// drawing order, six small blasts, four large blasts, six station blasts, then the Japanese PLANET ZEBES subtitle
    /// (#1733). Original
    /// sprite part counts describe stock geometry without restricting edited compositions.
    /// </summary>
    public static IReadOnlyList<CeresDestructionSpriteFrameDefinition> Frames { get; } =
    [
        new("station-under-attack-large-asteroid", Poses[0], 19),
        new("planet-zebes", Poses[1], 50),
        new("planet-zebes-title", Poses[2], 11),
        new("zebes-stars-upper-left", Poses[3], 12),
        new("zebes-stars-upper-right", Poses[4], 6),
        new("zebes-stars-lower-left", Poses[5], 4),
        new("zebes-stars-lower-right", Poses[6], 7),
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
        new("planet-zebes-subtitle", Poses[23], 12),
    ];
}
