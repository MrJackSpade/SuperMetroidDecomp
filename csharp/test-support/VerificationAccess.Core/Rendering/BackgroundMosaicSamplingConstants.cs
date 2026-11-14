namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge values and helpers of <see cref="BackgroundPixelation"/> that only verification reads.</summary>
internal static class BackgroundMosaicSamplingConstants
{
    /// <summary>MOSAIC ($2106): BG2 enable is bit one; the high nibble encodes size minus one.</summary>
    public static BackgroundPixelation ForBg2(byte register) =>
        (register & 2) == 0 ? default : new((register >> 4) + 1);

    /// <summary>MOSAIC ($2106): BG1 enable is bit zero; the high nibble encodes size minus one.</summary>
    public static BackgroundPixelation ForBg1(byte register) =>
        (register & 1) == 0 ? default : new((register >> 4) + 1);
}
