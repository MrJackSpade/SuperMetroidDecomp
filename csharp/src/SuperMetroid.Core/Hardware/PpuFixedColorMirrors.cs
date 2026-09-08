namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge WRAM mirrors written to COLDATA during NMI.</summary>
public static class PpuFixedColorMirrors
{
    /// <summary>$00:0074, DP_ColorMathSubScreenBackdropColor0, normally the red write.</summary>
    public const int Red = 0x74;
    /// <summary>$00:0075, DP_ColorMathSubScreenBackdropColor1; Fireflea deliberately writes blue here.</summary>
    public const int Green = 0x75;
    /// <summary>$00:0076, DP_ColorMathSubScreenBackdropColor2; Fireflea deliberately writes green here.</summary>
    public const int Blue = 0x76;
}
