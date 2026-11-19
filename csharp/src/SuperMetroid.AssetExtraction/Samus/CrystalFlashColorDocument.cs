using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Editable JSON payload containing Crystal Flash body and bubble RGB5 frames; native playback timing is not part of this schema.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the Crystal Flash's body and bubble colour rows, which only the import reads to pre-colour Samus's shades; MapPresentationExtractor.Extract writes it to the map presentation's import data (MapPresentationFiles.ImportColourFiles), which the installer deletes before it publishes.")]
public sealed record CrystalFlashColorDocument
{
    /// <summary>Schema revision, required to equal <see cref="CrystalFlashColorFile.Version"/> during loading.</summary>
    public required int Version { get; init; }
    /// <summary>Ten native body records in playback order, ten colors each.</summary>
    public required PaletteRgb5[][] Body { get; init; }
    /// <summary>Six independent bubble frames in playback order, six colors each.</summary>
    public required PaletteRgb5[][] Bubble { get; init; }
}
