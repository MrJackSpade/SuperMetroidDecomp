using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley;

/// <summary>Editable Ceres Ridley getaway zoom shades corresponding to $A6:B107 onward, excluding each native row's unused sixteenth word.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the Ceres Ridley floor's zoom colour rows, which only the import reads to pre-colour the floor's shades; MapPresentationExtractor.Extract writes it to the map presentation's import data (MapPresentationFiles.ImportColourFiles), which the installer deletes before it publishes.")]
public sealed record CeresRidleyMode7ColorDocument
{
    /// <summary>Schema revision required to equal <see cref="CeresRidleyFloorColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Nine zoom-high-byte rows, zero through eight, each with fifteen RGB5 colors.</summary>
    public required PaletteRgb5[][] ZoomRows { get; init; }
}
