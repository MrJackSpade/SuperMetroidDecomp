using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Editable map-highlight cycle schema: each frame's hold and highlight colour, rather than an executable palette program or palette rows (#1637).</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the map highlight cycle's colours, which only the import reads to pre-colour the highlight's shades; MapPresentationExtractor.Extract writes it to the map presentation's import data (MapPresentationFiles.ImportColourFiles), which the installer deletes before it publishes.")]
public sealed record MapPaletteCycleDocument
{
    /// <summary>Schema revision; loading requires <see cref="MapPaletteCycleFile.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Caller-owned mutable array of one through 255 frames in playback order; copied into compiled values when loaded.</summary>
    public required MapPaletteCycleFrame[] Frames { get; init; }
}
