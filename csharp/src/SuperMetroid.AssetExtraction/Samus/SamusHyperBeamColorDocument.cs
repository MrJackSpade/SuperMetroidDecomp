using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The import's ten full-body Hyper Beam colour rows (#1637), import data only the import pre-colours her shades from,
/// separate from the gameplay-owned cycle clock and acquisition state.
/// </summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: Samus's ten full-body Hyper Beam colour rows, which only the import reads to pre-colour her Hyper Beam shades; MapPresentationExtractor.Extract writes it to the map presentation's import data (MapPresentationFiles.ImportColourFiles), which the installer deletes before it publishes.")]
public sealed record SamusHyperBeamColorDocument
{
    /// <summary>Schema revision; loading requires version one from <see cref="SamusHyperBeamColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Ten ordered rows of sixteen nonnull RGB5 colors, corresponding to $91:D99E's descending sources $9B:A360..A240; includes color zero's copied payload even though OBJ index-zero pixels remain transparent.</summary>
    public required PaletteRgb5[][] Frames { get; init; }
}
