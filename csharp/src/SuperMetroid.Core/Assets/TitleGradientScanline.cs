namespace SuperMetroid.Core.Assets;

/// <summary>One scanline's effective fixed RGB5 color after native COLDATA writes, plus the CGADSUB byte governing its color arithmetic.</summary>
public sealed record TitleGradientScanline
{
    /// <summary>Fixed-color red intensity from 0 through 31, not an eight-bit display-channel value.</summary>
    public required int Red { get; init; }
    /// <summary>Fixed-color green intensity from 0 through 31, not an eight-bit display-channel value.</summary>
    public required int Green { get; init; }
    /// <summary>Fixed-color blue intensity from 0 through 31, not an eight-bit display-channel value.</summary>
    public required int Blue { get; init; }
    /// <summary>Raw CGADSUB value from 0 through 255. The title renderer uses bit 0 for BG1/backdrop, bit 4 for OBJ palettes 4..7 and bit 7 for subtraction; native $88:EB95 supplies $A1/$31 runs.</summary>
    public required int ColorMathControl { get; init; }
}
