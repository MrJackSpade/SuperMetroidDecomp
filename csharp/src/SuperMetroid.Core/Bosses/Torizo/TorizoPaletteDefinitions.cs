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
        new RgbColor(0, 0, 14), new RgbColor(31, 31, 0), new RgbColor(27, 25, 0), new RgbColor(22, 16, 0), new RgbColor(19, 8, 0), new RgbColor(30, 24, 26), new RgbColor(22, 16, 18), new RgbColor(17, 12, 13),
        new RgbColor(9, 7, 8), new RgbColor(0, 12, 5), new RgbColor(0, 12, 5), new RgbColor(0, 12, 5), new RgbColor(0, 12, 5), new RgbColor(0, 12, 5), new RgbColor(0, 12, 5), new RgbColor(0, 12, 5),
        new RgbColor(0, 0, 14), new RgbColor(31, 22, 0), new RgbColor(23, 14, 0), new RgbColor(12, 5, 0), new RgbColor(19, 19, 22), new RgbColor(13, 13, 16), new RgbColor(8, 8, 11), new RgbColor(3, 3, 6),
        new RgbColor(6, 4, 5), new RgbColor(5, 10, 0), new RgbColor(5, 10, 0), new RgbColor(5, 10, 0), new RgbColor(31, 31, 31), new RgbColor(5, 10, 0), new RgbColor(5, 10, 0), new RgbColor(0, 0, 0),
    ];
    /// <summary>$AA:86C7/$86E7, Bomb Torizo's initial (statue) body and belly colours.</summary>
    internal static ReadOnlySpan<RgbColor> BombInitial => BombInitialColors;
    /// <summary>Bomb Torizo's statue body colors before the normal-body transition.</summary>
    private static readonly RgbColor[] BombInitialColors =
    [
        new RgbColor(0, 0, 14), new RgbColor(31, 28, 25), new RgbColor(25, 20, 20), new RgbColor(14, 9, 9), new RgbColor(10, 5, 5), new RgbColor(28, 23, 23), new RgbColor(23, 18, 17), new RgbColor(18, 13, 13),
        new RgbColor(16, 11, 11), new RgbColor(31, 27, 22), new RgbColor(24, 15, 15), new RgbColor(14, 8, 11), new RgbColor(31, 26, 23), new RgbColor(26, 16, 23), new RgbColor(21, 9, 23), new RgbColor(3, 3, 3),
        new RgbColor(0, 0, 14), new RgbColor(26, 21, 18), new RgbColor(18, 13, 13), new RgbColor(7, 2, 2), new RgbColor(3, 0, 0), new RgbColor(21, 16, 16), new RgbColor(16, 11, 10), new RgbColor(11, 6, 6),
        new RgbColor(9, 4, 4), new RgbColor(26, 17, 17), new RgbColor(19, 5, 10), new RgbColor(9, 0, 6), new RgbColor(31, 27, 27), new RgbColor(29, 15, 20), new RgbColor(19, 8, 16), new RgbColor(3, 3, 3),
    ];
    /// <summary>$AA:8707/$8727, normal body targets loaded by $AA:C268.</summary>
    internal static ReadOnlySpan<RgbColor> Normal => NormalColors;
    /// <summary>Normal Bomb Torizo body targets for the selected-palette fade.</summary>
    private static readonly RgbColor[] NormalColors =
    [
        new RgbColor(0, 0, 14), new RgbColor(26, 21, 21), new RgbColor(18, 13, 16), new RgbColor(7, 2, 5), new RgbColor(3, 0, 1), new RgbColor(21, 16, 19), new RgbColor(16, 11, 13), new RgbColor(11, 6, 9),
        new RgbColor(8, 3, 6), new RgbColor(31, 27, 27), new RgbColor(24, 15, 20), new RgbColor(14, 8, 16), new RgbColor(31, 24, 0), new RgbColor(26, 14, 0), new RgbColor(21, 7, 0), new RgbColor(3, 3, 3),
        new RgbColor(0, 0, 14), new RgbColor(21, 16, 16), new RgbColor(13, 8, 11), new RgbColor(2, 0, 0), new RgbColor(0, 0, 0), new RgbColor(16, 11, 14), new RgbColor(11, 6, 8), new RgbColor(6, 1, 3),
        new RgbColor(3, 0, 1), new RgbColor(26, 17, 17), new RgbColor(19, 5, 10), new RgbColor(9, 0, 6), new RgbColor(31, 27, 27), new RgbColor(29, 15, 20), new RgbColor(19, 8, 16), new RgbColor(3, 3, 3),
    ];
    /// <summary>
    /// $AA:8747/$8767, the Golden encounter's initial body pair written by Torizo_C280 before
    /// the first live damage callback switches to bank $84's health-indexed gradient.
    /// </summary>
    internal static ReadOnlySpan<RgbColor> GoldenInitial => GoldenInitialColors;
    /// <summary>Golden Torizo's initial body pair before its first damage callback.</summary>
    private static readonly RgbColor[] GoldenInitialColors =
    [
        new RgbColor(0, 0, 14), new RgbColor(21, 21, 26), new RgbColor(16, 13, 18), new RgbColor(5, 2, 7), new RgbColor(1, 0, 3), new RgbColor(19, 16, 21), new RgbColor(13, 11, 16), new RgbColor(9, 6, 11),
        new RgbColor(6, 3, 8), new RgbColor(20, 24, 21), new RgbColor(12, 14, 12), new RgbColor(3, 7, 5), new RgbColor(16, 17, 21), new RgbColor(9, 11, 13), new RgbColor(3, 4, 6), new RgbColor(6, 3, 3),
        new RgbColor(0, 0, 14), new RgbColor(16, 16, 21), new RgbColor(11, 8, 13), new RgbColor(0, 0, 2), new RgbColor(0, 0, 0), new RgbColor(14, 11, 16), new RgbColor(8, 6, 11), new RgbColor(3, 1, 6),
        new RgbColor(1, 0, 3), new RgbColor(17, 17, 26), new RgbColor(10, 5, 19), new RgbColor(6, 0, 9), new RgbColor(27, 27, 31), new RgbColor(20, 15, 29), new RgbColor(16, 8, 19), new RgbColor(3, 3, 3),
    ];
    /// <summary>$AA:8787/$87A7, Golden body targets loaded by $AA:C298.</summary>
    internal static ReadOnlySpan<RgbColor> Golden => GoldenColors;
    /// <summary>Golden Torizo body targets loaded by its palette transition.</summary>
    private static readonly RgbColor[] GoldenColors =
    [
        new RgbColor(0, 0, 14), new RgbColor(30, 29, 18), new RgbColor(25, 21, 1), new RgbColor(8, 5, 0), new RgbColor(0, 0, 0), new RgbColor(26, 25, 5), new RgbColor(22, 19, 0), new RgbColor(18, 15, 0),
        new RgbColor(13, 10, 0), new RgbColor(0, 31, 28), new RgbColor(0, 25, 19), new RgbColor(0, 17, 10), new RgbColor(0, 31, 31), new RgbColor(0, 21, 22), new RgbColor(0, 9, 22), new RgbColor(3, 2, 0),
        new RgbColor(0, 0, 14), new RgbColor(25, 24, 13), new RgbColor(20, 16, 0), new RgbColor(3, 0, 0), new RgbColor(0, 0, 0), new RgbColor(21, 20, 0), new RgbColor(17, 14, 0), new RgbColor(13, 10, 0),
        new RgbColor(8, 5, 0), new RgbColor(0, 26, 18), new RgbColor(0, 15, 9), new RgbColor(0, 7, 0), new RgbColor(0, 26, 26), new RgbColor(0, 16, 17), new RgbColor(0, 4, 17), new RgbColor(0, 0, 0),
    ];
}
