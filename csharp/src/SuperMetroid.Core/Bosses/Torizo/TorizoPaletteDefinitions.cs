using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Targets and mask for Torizo's native palette-transition instructions.</summary>
internal static class TorizoPaletteDefinitions
{
    /// <summary>$82:DAFF supplies denominator 12 to the selected-palette fade.</summary>
    internal const int FadeDenominator = 12;
    /// <summary>
    /// $AA:8687/$86A7, the colours shared by both Torizos: orb/flame paint, then statue-shading
    /// paint. The orb/flame colours nine to fifteen and the shading colours nine to eleven,
    /// thirteen and fourteen are single repeated fill colors.
    /// </summary>
    internal static ReadOnlySpan<RgbColor> Shared => SharedColors;
    /// <summary>The orb/flame and statue-shading colours shared by Bomb and Golden Torizo.</summary>
    private static readonly RgbColor[] SharedColors =
    [
        RgbColor.FromWord(0x3800), RgbColor.FromWord(0x03ff), RgbColor.FromWord(0x033b), RgbColor.FromWord(0x0216), RgbColor.FromWord(0x0113), RgbColor.FromWord(0x6b1e), RgbColor.FromWord(0x4a16), RgbColor.FromWord(0x3591),
        RgbColor.FromWord(0x20e9), RgbColor.FromWord(0x1580), RgbColor.FromWord(0x1580), RgbColor.FromWord(0x1580), RgbColor.FromWord(0x1580), RgbColor.FromWord(0x1580), RgbColor.FromWord(0x1580), RgbColor.FromWord(0x1580),
        RgbColor.FromWord(0x3800), RgbColor.FromWord(0x02df), RgbColor.FromWord(0x01d7), RgbColor.FromWord(0x00ac), RgbColor.FromWord(0x5a73), RgbColor.FromWord(0x41ad), RgbColor.FromWord(0x2d08), RgbColor.FromWord(0x1863),
        RgbColor.FromWord(0x1486), RgbColor.FromWord(0x0145), RgbColor.FromWord(0x0145), RgbColor.FromWord(0x0145), RgbColor.FromWord(0x7fff), RgbColor.FromWord(0x0145), RgbColor.FromWord(0x0145), RgbColor.FromWord(0x0000),
    ];
    /// <summary>$AA:86C7/$86E7, Bomb Torizo's initial (statue) body and belly colours.</summary>
    internal static ReadOnlySpan<RgbColor> BombInitial => BombInitialColors;
    /// <summary>Bomb Torizo's statue body colors before the normal-body transition.</summary>
    private static readonly RgbColor[] BombInitialColors =
    [
        RgbColor.FromWord(0x3800), RgbColor.FromWord(0x679f), RgbColor.FromWord(0x5299), RgbColor.FromWord(0x252e), RgbColor.FromWord(0x14aa), RgbColor.FromWord(0x5efc), RgbColor.FromWord(0x4657), RgbColor.FromWord(0x35b2),
        RgbColor.FromWord(0x2d70), RgbColor.FromWord(0x5b7f), RgbColor.FromWord(0x3df8), RgbColor.FromWord(0x2d0e), RgbColor.FromWord(0x5f5f), RgbColor.FromWord(0x5e1a), RgbColor.FromWord(0x5d35), RgbColor.FromWord(0x0c63),
        RgbColor.FromWord(0x3800), RgbColor.FromWord(0x4aba), RgbColor.FromWord(0x35b2), RgbColor.FromWord(0x0847), RgbColor.FromWord(0x0003), RgbColor.FromWord(0x4215), RgbColor.FromWord(0x2970), RgbColor.FromWord(0x18cb),
        RgbColor.FromWord(0x1089), RgbColor.FromWord(0x463a), RgbColor.FromWord(0x28b3), RgbColor.FromWord(0x1809), RgbColor.FromWord(0x6f7f), RgbColor.FromWord(0x51fd), RgbColor.FromWord(0x4113), RgbColor.FromWord(0x0c63),
    ];
    /// <summary>$AA:8707/$8727, normal body targets loaded by $AA:C268.</summary>
    internal static ReadOnlySpan<RgbColor> Normal => NormalColors;
    /// <summary>Normal Bomb Torizo body targets for the selected-palette fade.</summary>
    private static readonly RgbColor[] NormalColors =
    [
        RgbColor.FromWord(0x3800), RgbColor.FromWord(0x56ba), RgbColor.FromWord(0x41b2), RgbColor.FromWord(0x1447), RgbColor.FromWord(0x0403), RgbColor.FromWord(0x4e15), RgbColor.FromWord(0x3570), RgbColor.FromWord(0x24cb),
        RgbColor.FromWord(0x1868), RgbColor.FromWord(0x6f7f), RgbColor.FromWord(0x51f8), RgbColor.FromWord(0x410e), RgbColor.FromWord(0x031f), RgbColor.FromWord(0x01da), RgbColor.FromWord(0x00f5), RgbColor.FromWord(0x0c63),
        RgbColor.FromWord(0x3800), RgbColor.FromWord(0x4215), RgbColor.FromWord(0x2d0d), RgbColor.FromWord(0x0002), RgbColor.FromWord(0x0000), RgbColor.FromWord(0x3970), RgbColor.FromWord(0x20cb), RgbColor.FromWord(0x0c26),
        RgbColor.FromWord(0x0403), RgbColor.FromWord(0x463a), RgbColor.FromWord(0x28b3), RgbColor.FromWord(0x1809), RgbColor.FromWord(0x6f7f), RgbColor.FromWord(0x51fd), RgbColor.FromWord(0x4113), RgbColor.FromWord(0x0c63),
    ];
    /// <summary>
    /// $AA:8747/$8767, the Golden encounter's initial body pair written by Torizo_C280 before
    /// the first live damage callback switches to bank $84's health-indexed gradient.
    /// </summary>
    internal static ReadOnlySpan<RgbColor> GoldenInitial => GoldenInitialColors;
    /// <summary>Golden Torizo's initial body pair before its first damage callback.</summary>
    private static readonly RgbColor[] GoldenInitialColors =
    [
        RgbColor.FromWord(0x3800), RgbColor.FromWord(0x6ab5), RgbColor.FromWord(0x49b0), RgbColor.FromWord(0x1c45), RgbColor.FromWord(0x0c01), RgbColor.FromWord(0x5613), RgbColor.FromWord(0x416d), RgbColor.FromWord(0x2cc9),
        RgbColor.FromWord(0x2066), RgbColor.FromWord(0x5714), RgbColor.FromWord(0x31cc), RgbColor.FromWord(0x14e3), RgbColor.FromWord(0x5630), RgbColor.FromWord(0x3569), RgbColor.FromWord(0x1883), RgbColor.FromWord(0x0c66),
        RgbColor.FromWord(0x3800), RgbColor.FromWord(0x5610), RgbColor.FromWord(0x350b), RgbColor.FromWord(0x0800), RgbColor.FromWord(0x0000), RgbColor.FromWord(0x416e), RgbColor.FromWord(0x2cc8), RgbColor.FromWord(0x1823),
        RgbColor.FromWord(0x0c01), RgbColor.FromWord(0x6a31), RgbColor.FromWord(0x4caa), RgbColor.FromWord(0x2406), RgbColor.FromWord(0x7f7b), RgbColor.FromWord(0x75f4), RgbColor.FromWord(0x4d10), RgbColor.FromWord(0x0c63),
    ];
    /// <summary>$AA:8787/$87A7, Golden body targets loaded by $AA:C298.</summary>
    internal static ReadOnlySpan<RgbColor> Golden => GoldenColors;
    /// <summary>Golden Torizo body targets loaded by its palette transition.</summary>
    private static readonly RgbColor[] GoldenColors =
    [
        RgbColor.FromWord(0x3800), RgbColor.FromWord(0x4bbe), RgbColor.FromWord(0x06b9), RgbColor.FromWord(0x00a8), RgbColor.FromWord(0x0000), RgbColor.FromWord(0x173a), RgbColor.FromWord(0x0276), RgbColor.FromWord(0x01f2),
        RgbColor.FromWord(0x014d), RgbColor.FromWord(0x73e0), RgbColor.FromWord(0x4f20), RgbColor.FromWord(0x2a20), RgbColor.FromWord(0x7fe0), RgbColor.FromWord(0x5aa0), RgbColor.FromWord(0x5920), RgbColor.FromWord(0x0043),
        RgbColor.FromWord(0x3800), RgbColor.FromWord(0x3719), RgbColor.FromWord(0x0214), RgbColor.FromWord(0x0003), RgbColor.FromWord(0x0000), RgbColor.FromWord(0x0295), RgbColor.FromWord(0x01d1), RgbColor.FromWord(0x014d),
        RgbColor.FromWord(0x00a8), RgbColor.FromWord(0x4b40), RgbColor.FromWord(0x25e0), RgbColor.FromWord(0x00e0), RgbColor.FromWord(0x6b40), RgbColor.FromWord(0x4600), RgbColor.FromWord(0x4480), RgbColor.FromWord(0x0000),
    ];
}
