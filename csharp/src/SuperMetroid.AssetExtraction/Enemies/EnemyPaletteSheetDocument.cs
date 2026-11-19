using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// One enemy's sixteen colours as the cartridge stores them (#1637), import data only the import pre-colours the enemy's
/// sheet from; the installation holds the pre-coloured sheet.
/// </summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: one enemy header's sixteen colours, which only the import reads to pre-colour that enemy's sheet; EnemyTileArtworkFiles.Extract writes it to the enemy tiles' import data, which the installer deletes before it publishes.")]
public sealed record EnemyPaletteSheetDocument
{
    /// <summary>Schema revision, which must be 1 for this loader.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly sixteen non-null RGB5 colors in native palette order, with each red, green, and blue channel in 0..31.</summary>
    public required PaletteRgb5[] Colors { get; init; }
}
