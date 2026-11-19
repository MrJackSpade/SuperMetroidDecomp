using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Editable RGB5 schema for the six shared X-ray and animated-room visor colors, excluding their native timing and byte-offset state.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the visor cycle's colours, which only the import reads to pre-colour Samus's visor shades; MapPresentationExtractor.Extract writes it to the map presentation's import data (MapPresentationFiles.ImportColourFiles), which the installer deletes before it publishes.")]
public sealed record SamusVisorColorDocument
{
    /// <summary>Schema revision; loading requires version one from <see cref="SamusVisorColorFile.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Six nonnull RGB5 colors with channels 0..31 in $9B:A3C0 word order: three widening colors followed by three full-X-ray/room-cycle colors.</summary>
    public required PaletteRgb5[] Colors { get; init; }
}
