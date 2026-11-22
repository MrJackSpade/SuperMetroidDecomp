using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The import's two complete Samus OBJ palettes (#1637), import data only the import pre-colours her hurt and intro shades
/// from; initialized array references and their RGB5 entries remain caller-owned and mutable.
/// </summary>
public sealed partial record SamusHurtColorDocument
{
    /// <summary>Schema version required to match <see cref="SamusHurtColorFile.Version"/> when loading or writing.</summary>
    public required int Version { get; init; }
    /// <summary>Sixteen RGB5 colors in native hurt-flash pen order, including transparent slot zero; replaces Samus OBJ palette four on odd flash calls.</summary>
    public required PaletteRgb5[] Hurt { get; init; }
    /// <summary>Sixteen RGB5 colors in native intro pen order, including transparent slot zero; supplies cinematic restoration rather than the equipment-dependent normal suit palette.</summary>
    public required PaletteRgb5[] Intro { get; init; }
}
