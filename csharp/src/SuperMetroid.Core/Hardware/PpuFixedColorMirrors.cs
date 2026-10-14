namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge WRAM mirrors written to COLDATA during NMI (#629).</summary>
[CpuBusBoundary("catalog: COLDATA fixed-color WRAM mirrors")]
public static class PpuFixedColorMirrors
{
    /// <summary>$00:0074, DP_ColorMathSubScreenBackdropColor0, normally the red write.</summary>
    public static readonly CpuBusCell Red = CpuBusCell.WramByte(nameof(Red), 0x000074, "COLDATA write (u8: channel bits | 5-bit intensity)");

    /// <summary>$00:0075, DP_ColorMathSubScreenBackdropColor1; Fireflea deliberately writes blue here.</summary>
    public static readonly CpuBusCell Green = CpuBusCell.WramByte(nameof(Green), 0x000075, "COLDATA write (u8: channel bits | 5-bit intensity)");

    /// <summary>$00:0076, DP_ColorMathSubScreenBackdropColor2; Fireflea deliberately writes green here.</summary>
    public static readonly CpuBusCell Blue = CpuBusCell.WramByte(nameof(Blue), 0x000076, "COLDATA write (u8: channel bits | 5-bit intensity)");
}
