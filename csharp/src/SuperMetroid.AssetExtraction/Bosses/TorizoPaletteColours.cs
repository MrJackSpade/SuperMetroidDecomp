using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// The colours of the Torizos' colourings (#1512), which the import pre-colours their sheets in: the colours both share,
/// each statue's initial body and belly colours, and the targets their fades reach.
/// </summary>
internal static class TorizoPaletteColours
{
    /// <summary>
    /// The colours shared by both Torizos: orb/flame paint, then statue-shading
    /// paint. The orb/flame colours nine to fifteen and the shading colours nine to eleven,
    /// thirteen and fourteen are single repeated fill colors.
    /// </summary>
    internal static ReadOnlySpan<RgbColor> Shared => SharedColors;
    /// <summary>The orb/flame and statue-shading colours shared by Bomb and Golden Torizo.</summary>
    private static readonly RgbColor[] SharedColors =
    [
        new RgbColor(0, 0, 115), new RgbColor(255, 255, 0), new RgbColor(222, 206, 0), new RgbColor(181, 132, 0), new RgbColor(156, 66, 0), new RgbColor(247, 198, 214), new RgbColor(181, 132, 148), new RgbColor(140, 99, 107),
        new RgbColor(74, 57, 66), new RgbColor(0, 99, 41), new RgbColor(0, 99, 41), new RgbColor(0, 99, 41), new RgbColor(0, 99, 41), new RgbColor(0, 99, 41), new RgbColor(0, 99, 41), new RgbColor(0, 99, 41),
        new RgbColor(0, 0, 115), new RgbColor(255, 181, 0), new RgbColor(189, 115, 0), new RgbColor(99, 41, 0), new RgbColor(156, 156, 181), new RgbColor(107, 107, 132), new RgbColor(66, 66, 90), new RgbColor(24, 24, 49),
        new RgbColor(49, 33, 41), new RgbColor(41, 82, 0), new RgbColor(41, 82, 0), new RgbColor(41, 82, 0), new RgbColor(255, 255, 255), new RgbColor(41, 82, 0), new RgbColor(41, 82, 0), new RgbColor(0, 0, 0),
    ];
    /// <summary>Bomb Torizo's initial (statue) body and belly colours.</summary>
    internal static ReadOnlySpan<RgbColor> BombInitial => BombInitialColors;
    /// <summary>Bomb Torizo's statue body colors before the normal-body transition.</summary>
    private static readonly RgbColor[] BombInitialColors =
    [
        new RgbColor(0, 0, 115), new RgbColor(255, 231, 206), new RgbColor(206, 165, 165), new RgbColor(115, 74, 74), new RgbColor(82, 41, 41), new RgbColor(231, 189, 189), new RgbColor(189, 148, 140), new RgbColor(148, 107, 107),
        new RgbColor(132, 90, 90), new RgbColor(255, 222, 181), new RgbColor(198, 123, 123), new RgbColor(115, 66, 90), new RgbColor(255, 214, 189), new RgbColor(214, 132, 189), new RgbColor(173, 74, 189), new RgbColor(24, 24, 24),
        new RgbColor(0, 0, 115), new RgbColor(214, 173, 148), new RgbColor(148, 107, 107), new RgbColor(57, 16, 16), new RgbColor(24, 0, 0), new RgbColor(173, 132, 132), new RgbColor(132, 90, 82), new RgbColor(90, 49, 49),
        new RgbColor(74, 33, 33), new RgbColor(214, 140, 140), new RgbColor(156, 41, 82), new RgbColor(74, 0, 49), new RgbColor(255, 222, 222), new RgbColor(239, 123, 165), new RgbColor(156, 66, 132), new RgbColor(24, 24, 24),
    ];
    /// <summary>Normal body targets loaded by Bomb Torizo's palette setup.</summary>
    internal static ReadOnlySpan<RgbColor> Normal => NormalColors;
    /// <summary>Normal Bomb Torizo body targets for the selected-palette fade.</summary>
    private static readonly RgbColor[] NormalColors =
    [
        new RgbColor(0, 0, 115), new RgbColor(214, 173, 173), new RgbColor(148, 107, 132), new RgbColor(57, 16, 41), new RgbColor(24, 0, 8), new RgbColor(173, 132, 156), new RgbColor(132, 90, 107), new RgbColor(90, 49, 74),
        new RgbColor(66, 24, 49), new RgbColor(255, 222, 222), new RgbColor(198, 123, 165), new RgbColor(115, 66, 132), new RgbColor(255, 198, 0), new RgbColor(214, 115, 0), new RgbColor(173, 57, 0), new RgbColor(24, 24, 24),
        new RgbColor(0, 0, 115), new RgbColor(173, 132, 132), new RgbColor(107, 66, 90), new RgbColor(16, 0, 0), new RgbColor(0, 0, 0), new RgbColor(132, 90, 115), new RgbColor(90, 49, 66), new RgbColor(49, 8, 24),
        new RgbColor(24, 0, 8), new RgbColor(214, 140, 140), new RgbColor(156, 41, 82), new RgbColor(74, 0, 49), new RgbColor(255, 222, 222), new RgbColor(239, 123, 165), new RgbColor(156, 66, 132), new RgbColor(24, 24, 24),
    ];
    /// <summary>
    /// The Golden encounter's initial body pair written by Torizo_C280 before
    /// the first live damage callback switches to the health-indexed gradient.
    /// </summary>
    internal static ReadOnlySpan<RgbColor> GoldenInitial => GoldenInitialColors;
    /// <summary>Golden Torizo's initial body pair before its first damage callback.</summary>
    private static readonly RgbColor[] GoldenInitialColors =
    [
        new RgbColor(0, 0, 115), new RgbColor(173, 173, 214), new RgbColor(132, 107, 148), new RgbColor(41, 16, 57), new RgbColor(8, 0, 24), new RgbColor(156, 132, 173), new RgbColor(107, 90, 132), new RgbColor(74, 49, 90),
        new RgbColor(49, 24, 66), new RgbColor(165, 198, 173), new RgbColor(99, 115, 99), new RgbColor(24, 57, 41), new RgbColor(132, 140, 173), new RgbColor(74, 90, 107), new RgbColor(24, 33, 49), new RgbColor(49, 24, 24),
        new RgbColor(0, 0, 115), new RgbColor(132, 132, 173), new RgbColor(90, 66, 107), new RgbColor(0, 0, 16), new RgbColor(0, 0, 0), new RgbColor(115, 90, 132), new RgbColor(66, 49, 90), new RgbColor(24, 8, 49),
        new RgbColor(8, 0, 24), new RgbColor(140, 140, 214), new RgbColor(82, 41, 156), new RgbColor(49, 0, 74), new RgbColor(222, 222, 255), new RgbColor(165, 123, 239), new RgbColor(132, 66, 156), new RgbColor(24, 24, 24),
    ];
    /// <summary>Golden body targets loaded by Golden Torizo's palette setup.</summary>
    internal static ReadOnlySpan<RgbColor> Golden => GoldenColors;
    /// <summary>Golden Torizo body targets loaded by its palette transition.</summary>
    private static readonly RgbColor[] GoldenColors =
    [
        new RgbColor(0, 0, 115), new RgbColor(247, 239, 148), new RgbColor(206, 173, 8), new RgbColor(66, 41, 0), new RgbColor(0, 0, 0), new RgbColor(214, 206, 41), new RgbColor(181, 156, 0), new RgbColor(148, 123, 0),
        new RgbColor(107, 82, 0), new RgbColor(0, 255, 231), new RgbColor(0, 206, 156), new RgbColor(0, 140, 82), new RgbColor(0, 255, 255), new RgbColor(0, 173, 181), new RgbColor(0, 74, 181), new RgbColor(24, 16, 0),
        new RgbColor(0, 0, 115), new RgbColor(206, 198, 107), new RgbColor(165, 132, 0), new RgbColor(24, 0, 0), new RgbColor(0, 0, 0), new RgbColor(173, 165, 0), new RgbColor(140, 115, 0), new RgbColor(107, 82, 0),
        new RgbColor(66, 41, 0), new RgbColor(0, 214, 148), new RgbColor(0, 123, 74), new RgbColor(0, 57, 0), new RgbColor(0, 214, 214), new RgbColor(0, 132, 140), new RgbColor(0, 33, 140), new RgbColor(0, 0, 0),
    ];
}
