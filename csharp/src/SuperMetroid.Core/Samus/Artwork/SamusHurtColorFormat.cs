using SuperMetroid.Core.Samus.Palettes;


namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Installed JSON identity, schema dimensions, and extraction-only cartridge locations for Samus's hurt and cinematic full-body colors.</summary>
public static class SamusHurtColorFormat
{
    /// <summary>Installed resource filename loaded by the area-map presentation catalog for hurt-flash and cinematic restoration artwork.</summary>
    public const string FileName = "samus-hurt-colors.json";
    /// <summary>Supported schema version, requiring both complete sixteen-color RGB5 arrays.</summary>
    public const int Version = 1;
    /// <summary>Number of colors in one SNES OBJ palette: sixteen, including the transparent first slot.</summary>
    public const int ColorsPerPalette = SamusPaletteRomData.Common.ColorsPerObjPalette;
    /// <summary>Extraction address <c>$9B:A380</c>, native <c>SamusPalettes_HurtFlash</c>: sixteen little-endian SNES color words copied on odd hurt-counter calls below seven.</summary>
    public const int HurtSourceAddress = SamusPaletteRomData.HurtFlash.Colors;
    /// <summary>Extraction address <c>$9B:A3A0</c>, native <c>SamusPalettes_Intro</c>: sixteen little-endian SNES color words used for cinematic restoration, distinct from normal suit artwork.</summary>
    public const int IntroSourceAddress = SamusPaletteRomData.HurtFlash.IntroColors;
}
