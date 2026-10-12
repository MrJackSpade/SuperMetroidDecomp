using SuperMetroid.Core.Hardware;

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
    /// <summary>
    /// $AA:8687/$86A7, sprite palettes 3 and 7 shared by both Torizos (CGRAM rows eleven and
    /// fifteen): orb/flame and statue-shading paint. Row eleven's colors nine to fifteen and
    /// row fifteen's nine to eleven, thirteen and fourteen are single repeated fill colors.
    /// </summary>
    internal static ReadOnlySpan<Bgr555> SharedRows => SharedRowsColors;
    private static readonly Bgr555[] SharedRowsColors =
    [
        Bgr555.FromWord(0x3800), Bgr555.FromWord(0x03ff), Bgr555.FromWord(0x033b), Bgr555.FromWord(0x0216), Bgr555.FromWord(0x0113), Bgr555.FromWord(0x6b1e), Bgr555.FromWord(0x4a16), Bgr555.FromWord(0x3591),
        Bgr555.FromWord(0x20e9), Bgr555.FromWord(0x1580), Bgr555.FromWord(0x1580), Bgr555.FromWord(0x1580), Bgr555.FromWord(0x1580), Bgr555.FromWord(0x1580), Bgr555.FromWord(0x1580), Bgr555.FromWord(0x1580),
        Bgr555.FromWord(0x3800), Bgr555.FromWord(0x02df), Bgr555.FromWord(0x01d7), Bgr555.FromWord(0x00ac), Bgr555.FromWord(0x5a73), Bgr555.FromWord(0x41ad), Bgr555.FromWord(0x2d08), Bgr555.FromWord(0x1863),
        Bgr555.FromWord(0x1486), Bgr555.FromWord(0x0145), Bgr555.FromWord(0x0145), Bgr555.FromWord(0x0145), Bgr555.FromWord(0x7fff), Bgr555.FromWord(0x0145), Bgr555.FromWord(0x0145), Bgr555.FromWord(0x0000),
    ];
    /// <summary>$AA:86C7/$86E7, Bomb Torizo's initial (statue) body rows nine and ten.</summary>
    internal static ReadOnlySpan<Bgr555> BombInitial => BombInitialColors;
    private static readonly Bgr555[] BombInitialColors =
    [
        Bgr555.FromWord(0x3800), Bgr555.FromWord(0x679f), Bgr555.FromWord(0x5299), Bgr555.FromWord(0x252e), Bgr555.FromWord(0x14aa), Bgr555.FromWord(0x5efc), Bgr555.FromWord(0x4657), Bgr555.FromWord(0x35b2),
        Bgr555.FromWord(0x2d70), Bgr555.FromWord(0x5b7f), Bgr555.FromWord(0x3df8), Bgr555.FromWord(0x2d0e), Bgr555.FromWord(0x5f5f), Bgr555.FromWord(0x5e1a), Bgr555.FromWord(0x5d35), Bgr555.FromWord(0x0c63),
        Bgr555.FromWord(0x3800), Bgr555.FromWord(0x4aba), Bgr555.FromWord(0x35b2), Bgr555.FromWord(0x0847), Bgr555.FromWord(0x0003), Bgr555.FromWord(0x4215), Bgr555.FromWord(0x2970), Bgr555.FromWord(0x18cb),
        Bgr555.FromWord(0x1089), Bgr555.FromWord(0x463a), Bgr555.FromWord(0x28b3), Bgr555.FromWord(0x1809), Bgr555.FromWord(0x6f7f), Bgr555.FromWord(0x51fd), Bgr555.FromWord(0x4113), Bgr555.FromWord(0x0c63),
    ];
    /// <summary>$AA:8707/$8727, normal body targets loaded by $AA:C268.</summary>
    internal static ReadOnlySpan<Bgr555> Normal => NormalColors;
    private static readonly Bgr555[] NormalColors =
    [
        Bgr555.FromWord(0x3800), Bgr555.FromWord(0x56ba), Bgr555.FromWord(0x41b2), Bgr555.FromWord(0x1447), Bgr555.FromWord(0x0403), Bgr555.FromWord(0x4e15), Bgr555.FromWord(0x3570), Bgr555.FromWord(0x24cb),
        Bgr555.FromWord(0x1868), Bgr555.FromWord(0x6f7f), Bgr555.FromWord(0x51f8), Bgr555.FromWord(0x410e), Bgr555.FromWord(0x031f), Bgr555.FromWord(0x01da), Bgr555.FromWord(0x00f5), Bgr555.FromWord(0x0c63),
        Bgr555.FromWord(0x3800), Bgr555.FromWord(0x4215), Bgr555.FromWord(0x2d0d), Bgr555.FromWord(0x0002), Bgr555.FromWord(0x0000), Bgr555.FromWord(0x3970), Bgr555.FromWord(0x20cb), Bgr555.FromWord(0x0c26),
        Bgr555.FromWord(0x0403), Bgr555.FromWord(0x463a), Bgr555.FromWord(0x28b3), Bgr555.FromWord(0x1809), Bgr555.FromWord(0x6f7f), Bgr555.FromWord(0x51fd), Bgr555.FromWord(0x4113), Bgr555.FromWord(0x0c63),
    ];
    /// <summary>
    /// $AA:8747/$8767, the Golden encounter's initial body pair written by Torizo_C280 before
    /// the first live damage callback switches to bank $84's health-indexed gradient.
    /// </summary>
    internal static ReadOnlySpan<Bgr555> GoldenInitial => GoldenInitialColors;
    private static readonly Bgr555[] GoldenInitialColors =
    [
        Bgr555.FromWord(0x3800), Bgr555.FromWord(0x6ab5), Bgr555.FromWord(0x49b0), Bgr555.FromWord(0x1c45), Bgr555.FromWord(0x0c01), Bgr555.FromWord(0x5613), Bgr555.FromWord(0x416d), Bgr555.FromWord(0x2cc9),
        Bgr555.FromWord(0x2066), Bgr555.FromWord(0x5714), Bgr555.FromWord(0x31cc), Bgr555.FromWord(0x14e3), Bgr555.FromWord(0x5630), Bgr555.FromWord(0x3569), Bgr555.FromWord(0x1883), Bgr555.FromWord(0x0c66),
        Bgr555.FromWord(0x3800), Bgr555.FromWord(0x5610), Bgr555.FromWord(0x350b), Bgr555.FromWord(0x0800), Bgr555.FromWord(0x0000), Bgr555.FromWord(0x416e), Bgr555.FromWord(0x2cc8), Bgr555.FromWord(0x1823),
        Bgr555.FromWord(0x0c01), Bgr555.FromWord(0x6a31), Bgr555.FromWord(0x4caa), Bgr555.FromWord(0x2406), Bgr555.FromWord(0x7f7b), Bgr555.FromWord(0x75f4), Bgr555.FromWord(0x4d10), Bgr555.FromWord(0x0c63),
    ];
    /// <summary>$AA:8787/$87A7, Golden body targets loaded by $AA:C298.</summary>
    internal static ReadOnlySpan<Bgr555> Golden => GoldenColors;
    private static readonly Bgr555[] GoldenColors =
    [
        Bgr555.FromWord(0x3800), Bgr555.FromWord(0x4bbe), Bgr555.FromWord(0x06b9), Bgr555.FromWord(0x00a8), Bgr555.FromWord(0x0000), Bgr555.FromWord(0x173a), Bgr555.FromWord(0x0276), Bgr555.FromWord(0x01f2),
        Bgr555.FromWord(0x014d), Bgr555.FromWord(0x73e0), Bgr555.FromWord(0x4f20), Bgr555.FromWord(0x2a20), Bgr555.FromWord(0x7fe0), Bgr555.FromWord(0x5aa0), Bgr555.FromWord(0x5920), Bgr555.FromWord(0x0043),
        Bgr555.FromWord(0x3800), Bgr555.FromWord(0x3719), Bgr555.FromWord(0x0214), Bgr555.FromWord(0x0003), Bgr555.FromWord(0x0000), Bgr555.FromWord(0x0295), Bgr555.FromWord(0x01d1), Bgr555.FromWord(0x014d),
        Bgr555.FromWord(0x00a8), Bgr555.FromWord(0x4b40), Bgr555.FromWord(0x25e0), Bgr555.FromWord(0x00e0), Bgr555.FromWord(0x6b40), Bgr555.FromWord(0x4600), Bgr555.FromWord(0x4480), Bgr555.FromWord(0x0000),
    ];
}
