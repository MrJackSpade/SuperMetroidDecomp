using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The import's four auxiliary enemy colour families (#1637), import data only the import pre-colours their sheets' shades
/// from; runtime AI retains update cadence, drain-stage progression, and health-band selection.
/// </summary>
public sealed partial record EnemyAuxiliaryColorDocument
{
    /// <summary>Schema revision, which must equal <see cref="EnemyAuxiliaryColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>All four enum-named RGB5 families: FaceBlock has eight four-color rows, DeadSidehopper seven fifteen-color rows, and each Golden Torizo family eight sixteen-color health bands.</summary>
    public required Dictionary<EnemyAuxiliaryPalette, PaletteRgb5[][]> Palettes { get; init; }
}
