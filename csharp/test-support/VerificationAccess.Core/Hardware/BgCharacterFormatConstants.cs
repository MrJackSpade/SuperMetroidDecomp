using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge values and helpers of <see cref="BgCharacterFormat"/> that only verification reads.</summary>
internal static class BgCharacterFormatConstants
{
    /// <summary>Two bits per pixel: four colors per palette (BG3 in mode 1).</summary>
    public static BgCharacterFormat TwoBitsPerPixel { get; } = new(2);
}
