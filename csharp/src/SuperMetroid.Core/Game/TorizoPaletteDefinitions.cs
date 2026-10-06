namespace SuperMetroid.Core.Game;

/// <summary>Targets and mask for Torizo's native palette-transition instructions.</summary>
internal static class TorizoPaletteDefinitions
{
    /// <summary>$AA:B271 selects sprite palettes 1 and 2 via mask $0600.</summary>
    internal const ushort BodyPaletteMask = 0x0600;
    /// <summary>$82:DAFF supplies denominator 12 to the selected-palette fade.</summary>
    internal const int FadeDenominator = 12;
    /// <summary>$AA:C26F/$C276 target CGRAM rows 9 and 10, beginning at color 144.</summary>
    internal const int FirstBodyColor = 144;
    /// <summary>$AA:8707/$8727, normal body targets loaded by $AA:C268.</summary>
    internal static ReadOnlySpan<ushort> Normal =>
    [
        0x3800, 0x56ba, 0x41b2, 0x1447, 0x0403, 0x4e15, 0x3570, 0x24cb,
        0x1868, 0x6f7f, 0x51f8, 0x410e, 0x031f, 0x01da, 0x00f5, 0x0c63,
        0x3800, 0x4215, 0x2d0d, 0x0002, 0x0000, 0x3970, 0x20cb, 0x0c26,
        0x0403, 0x463a, 0x28b3, 0x1809, 0x6f7f, 0x51fd, 0x4113, 0x0c63,
    ];
    /// <summary>$AA:8787/$87A7, Golden body targets loaded by $AA:C298.</summary>
    internal static ReadOnlySpan<ushort> Golden =>
    [
        0x3800, 0x4bbe, 0x06b9, 0x00a8, 0x0000, 0x173a, 0x0276, 0x01f2,
        0x014d, 0x73e0, 0x4f20, 0x2a20, 0x7fe0, 0x5aa0, 0x5920, 0x0043,
        0x3800, 0x3719, 0x0214, 0x0003, 0x0000, 0x0295, 0x01d1, 0x014d,
        0x00a8, 0x4b40, 0x25e0, 0x00e0, 0x6b40, 0x4600, 0x4480, 0x0000,
    ];
}
