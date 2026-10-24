
namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge values and helpers of <see cref="BgCharacterFormat"/> that only verification reads.</summary>
internal static class BgCharacterFormatConstants
{
    /// <summary>Two bits per pixel: four colors per palette (BG3 in mode 1).</summary>
    public static BgCharacterFormat TwoBitsPerPixel { get; } = new(2);
    /// <summary>Palettes a tilemap entry's three-bit palette field can select.</summary>
    public const int PaletteCount = 8;
}
