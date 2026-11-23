using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Editable RGB5 schema for the six shared X-ray and animated-room visor colors, excluding their native timing and byte-offset state.</summary>
public sealed partial record SamusVisorColorDocument
{
    /// <summary>Schema revision; loading requires version one from <see cref="SamusVisorColorFile.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>The visor's three colours as the X-ray beam widens, in the order it shows them; RGB5 channels 0..31.</summary>
    public required PaletteRgb5[] Widening { get; init; }

    /// <summary>The visor's three colours as the full X-ray and an animated room cycle them, in the order they show them; RGB5 channels 0..31.</summary>
    public required PaletteRgb5[] Cycle { get; init; }

    /// <summary>All six colours, the widening's then the cycle's, as the visor's colour reader lists them.</summary>
    internal PaletteRgb5[] InReaderOrder() => [.. Widening, .. Cycle];
}
