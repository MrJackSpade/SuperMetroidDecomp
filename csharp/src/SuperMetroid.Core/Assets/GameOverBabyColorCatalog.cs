using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Game-over Baby color inputs with calculated cry-phase brightness steps.</summary>
/// <remarks>
/// Native $82:BD97..BE16 contains four sixteen-color phases. Middle/Open cry brighten
/// the preceding cry phase by three independently saturated RGB5 steps. Color zero
/// instead aliases Idle color zero. Import stores only differing target channels,
/// so independently edited source and target colors remain exact. Stock input consists
/// of 18 base words and five differing components, whose separate review remains open;
/// this conversion does not assert that those inputs qualify for retention.
/// Idle/ClosedCry green inks 2..4 share red/blue and have green levels spaced by five;
/// derive inks 2/3 from darkest ink 4, saturating for independently edited assets.
/// ClosedCry inks 5..12 preserve Idle green and shift five levels from blue to red.
/// Ink 9 red differs by one and remains an unresolved independently stored component.
/// Idle ink 11 is the component-wise floor midpoint between shade endpoints 10/12.
/// </remarks>
internal sealed class GameOverBabyColorCatalog
{
    private readonly Dictionary<int, ushort> inputs = new();
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> differences = new();

    /// <summary>Captures values from the four named palettes already validated by the presentation loader.</summary>
    internal GameOverBabyColorCatalog(IReadOnlyDictionary<string, ushort[]> palettes)
    {
        for (int phase = 0; phase < 4; phase++)
        for (int color = 0; color < 16; color++)
        {
            ushort supplied = palettes[GameOverPresentationDefinitions.BabyPaletteName((GameOverBabyPalette)phase)][color];
            int key = phase * 16 + color;
            bool greenShade = phase < 2 && color is 2 or 3;
            bool warmCry = phase == 1 && color is >= 5 and <= 12;
            bool middleShade = phase == 0 && color == 11;
            if (!greenShade && !warmCry && !middleShade && (phase == 0 || (phase == 1 && color != 0)))
            {
                inputs.Add(key, supplied);
                continue;
            }
            ushort expected = middleShade
                ? MiddleShade(palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.Idle)][10],
                    palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.Idle)][12])
                : greenShade
                ? GreenShade(palettes[GameOverPresentationDefinitions.BabyPaletteName((GameOverBabyPalette)phase)][4], color)
                : warmCry ? WarmCry(palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.Idle)][color])
                : color == 0
                ? palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.Idle)][0]
                : BrightenCry(palettes[GameOverPresentationDefinitions.BabyPaletteName((GameOverBabyPalette)(phase - 1))][color]);
            if (supplied != expected)
                differences.Add(key, new(supplied, expected));
        }
    }

    internal ushort Read(GameOverBabyPalette palette, int color)
    {
        _ = GameOverPresentationDefinitions.BabyPaletteName(palette);
        if ((uint)color >= 16) throw new IndexOutOfRangeException();
        int phase = (int)palette;
        int key = phase * 16 + color;
        if (inputs.TryGetValue(key, out ushort value)) return value;
        ushort expected = phase == 0 && color == 11
            ? MiddleShade(Read(palette, 10), Read(palette, 12))
            : phase < 2 && color is 2 or 3
            ? GreenShade(Read(palette, 4), color)
            : phase == 1 && color is >= 5 and <= 12 ? WarmCry(Read(GameOverBabyPalette.Idle, color))
            : color == 0 ? inputs[0] : BrightenCry(Read((GameOverBabyPalette)(phase - 1), color));
        return differences.TryGetValue(key, out var channels) ? channels.Apply(expected) : expected;
    }

    private static ushort GreenShade(ushort darkest, int color) =>
        (ushort)((darkest & 0x7c1f) | Math.Min(31, (darkest >> 5 & 31) + (4 - color) * 5) << 5);

    private static ushort WarmCry(ushort idle) =>
        (ushort)(Math.Min(31, (idle & 31) + 5) | (idle & 0x03e0) |
            Math.Max(0, (idle >> 10 & 31) - 5) << 10);

    private static ushort MiddleShade(ushort bright, ushort dark) =>
        (ushort)(((bright & 31) + (dark & 31)) / 2 |
            ((bright >> 5 & 31) + (dark >> 5 & 31)) / 2 << 5 |
            ((bright >> 10 & 31) + (dark >> 10 & 31)) / 2 << 10);

    /// <summary>RGB5 additive brightness, +3 per component, saturating each component at 31 before packing.</summary>
    internal static ushort BrightenCry(ushort color)
    {
        if (color > 0x7fff) throw new ArgumentOutOfRangeException(nameof(color));
        return (ushort)(Math.Min(31, (color & 31) + 3) |
            Math.Min(31, (color >> 5 & 31) + 3) << 5 |
            Math.Min(31, (color >> 10 & 31) + 3) << 10);
    }
}
