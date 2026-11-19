using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The import's four auxiliary enemy colour families (#1637), import data only the import pre-colours their sheets' shades
/// from; runtime AI retains update cadence, drain-stage progression, and health-band selection.
/// </summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the face blocks', dead Sidehopper's and Golden Torizo's colour rows, which only the import reads to pre-colour their sheets' shades; EnemyTileArtworkFiles.Extract writes it to the enemy tiles' import data, which the installer deletes before it publishes.")]
public sealed record EnemyAuxiliaryColorDocument
{
    /// <summary>Schema revision, which must equal <see cref="EnemyAuxiliaryColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>All four enum-named RGB5 families: FaceBlock has eight four-color rows, DeadSidehopper seven fifteen-color rows, and each Golden Torizo family eight sixteen-color health bands.</summary>
    public required Dictionary<EnemyAuxiliaryPalette, PaletteRgb5[][]> Palettes { get; init; }
}
