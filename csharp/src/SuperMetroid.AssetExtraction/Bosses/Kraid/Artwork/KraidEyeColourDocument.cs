using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;

/// <summary>
/// The installed colours of Kraid's three eyes (#1637), as a player edits them in place: the colours gameplay's eye glow
/// shows, starts from and fades back to under the hurt flash and under each of his eight health bands. The rest of his
/// colours are import data, which only pre-colours his pictures.
/// </summary>
public sealed record KraidEyeColourDocument
{
    /// <summary>Schema revision; loading requires <see cref="KraidEyeColourFile.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>The three eyes' colours under the hurt flash, first eye first.</summary>
    public required PaletteRgb5[] HurtFlash { get; init; }

    /// <summary>The three eyes' colours under each of the eight health bands, lowest band first and first eye first.</summary>
    public required PaletteRgb5[][] HealthBands { get; init; }
}
