using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Game-over Baby color inputs with calculated cry-phase brightness steps.</summary>
/// <remarks>
/// Native $82:BD97..BE16 contains four sixteen-color phases. Middle/Open cry brighten
/// the preceding cry phase by three independently saturated RGB5 steps. Color zero
/// instead aliases Idle color zero. Import stores only differing target channels,
/// so independently edited source and target colors remain exact. Stock input consists
/// of 14 base words and four differing components, whose separate review remains open;
/// this conversion does not assert that those inputs qualify for retention.
/// Idle/ClosedCry green inks 2..4 share red/blue and have green levels spaced by five;
/// derive inks 2/3 from darkest ink 4, saturating for independently edited assets.
/// ClosedCry inks 5..12 preserve Idle green and shift five levels from blue to red.
/// Ink 9 red differs by one and remains an unresolved independently stored component.
/// Idle ink 11 is the component-wise floor midpoint between shade endpoints 10/12.
/// ClosedCry cyan inks 1/15 add five red/green levels and subtract five blue levels;
/// ink 15 red remains independently supplied. The green ramp also adds five green
/// levels from Idle, with its red/blue endpoint choices still independently supplied.
/// Native OBJ tiles $90/$92/$9B use inks 10..12 for fangs and 1/13/14/15 for
/// the surrounding glass. MiddleCry holds fang red while lifting green/blue;
/// OpenCry lifts all three. Glass shade 13 stays at least one level below highlight
/// 14 in each channel. ClosedCry highlight 14 brightens Idle by five saturated steps.
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
            bool coolCry = phase == 1 && color is 1 or 15;
            bool greenCry = phase == 1 && color == 4;
            bool glassHighlight = phase == 1 && color == 14;
            if (!greenShade && !warmCry && !middleShade && !coolCry && !greenCry && !glassHighlight &&
                (phase == 0 || (phase == 1 && color != 0)))
            {
                inputs.Add(key, supplied);
                continue;
            }
            ushort expected = glassHighlight
                ? Brighten(palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.Idle)][color], 5)
                : coolCry
                ? CoolCry(palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.Idle)][color])
                : greenCry ? GreenCry(palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.Idle)][color])
                : middleShade
                ? MiddleShade(palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.Idle)][10],
                    palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.Idle)][12])
                : greenShade
                ? GreenShade(palettes[GameOverPresentationDefinitions.BabyPaletteName((GameOverBabyPalette)phase)][4], color)
                : warmCry ? WarmCry(palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.Idle)][color])
                : color == 0
                ? palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.Idle)][0]
                : CryShade(palettes[GameOverPresentationDefinitions.BabyPaletteName((GameOverBabyPalette)(phase - 1))][color],
                    phase, color, color == 13 ? palettes[GameOverPresentationDefinitions.BabyPaletteName((GameOverBabyPalette)phase)][14] : (ushort)0);
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
        ushort expected = phase == 1 && color == 14
            ? Brighten(Read(GameOverBabyPalette.Idle, color), 5)
            : phase == 1 && color is 1 or 15
            ? CoolCry(Read(GameOverBabyPalette.Idle, color))
            : phase == 1 && color == 4 ? GreenCry(Read(GameOverBabyPalette.Idle, color))
            : phase == 0 && color == 11
            ? MiddleShade(Read(palette, 10), Read(palette, 12))
            : phase < 2 && color is 2 or 3
            ? GreenShade(Read(palette, 4), color)
            : phase == 1 && color is >= 5 and <= 12 ? WarmCry(Read(GameOverBabyPalette.Idle, color))
            : color == 0 ? inputs[0] : CryShade(Read((GameOverBabyPalette)(phase - 1), color),
                phase, color, color == 13 ? Read(palette, 14) : (ushort)0);
        return differences.TryGetValue(key, out var channels) ? channels.Apply(expected) : expected;
    }

    private static ushort GreenShade(ushort darkest, int color) =>
        (ushort)((darkest & 0x7c1f) | Math.Min(31, (darkest >> 5 & 31) + (4 - color) * 5) << 5);

    private static ushort WarmCry(ushort idle) =>
        (ushort)(Math.Min(31, (idle & 31) + 5) | (idle & 0x03e0) |
            Math.Max(0, (idle >> 10 & 31) - 5) << 10);

    private static ushort CoolCry(ushort idle) =>
        (ushort)((WarmCry(idle) & 0x7c1f) | Math.Min(31, (idle >> 5 & 31) + 5) << 5);

    private static ushort GreenCry(ushort idle) =>
        (ushort)((idle & 0x7c1f) | Math.Min(31, (idle >> 5 & 31) + 5) << 5);

    private static ushort MiddleShade(ushort bright, ushort dark) =>
        (ushort)(((bright & 31) + (dark & 31)) / 2 |
            ((bright >> 5 & 31) + (dark >> 5 & 31)) / 2 << 5 |
            ((bright >> 10 & 31) + (dark >> 10 & 31)) / 2 << 10);

    private static ushort CryShade(ushort previous, int phase, int ink, ushort highlight)
    {
        ushort bright = Brighten(previous, 3);
        if (phase == 2 && ink is >= 10 and <= 12)
            return (ushort)((bright & 0x7fe0) | (previous & 31));
        if (ink == 13)
            return (ushort)(Math.Min(bright & 31, Math.Max(0, (highlight & 31) - 1)) |
                Math.Min(bright >> 5 & 31, Math.Max(0, (highlight >> 5 & 31) - 1)) << 5 |
                Math.Min(bright >> 10 & 31, Math.Max(0, (highlight >> 10 & 31) - 1)) << 10);
        return bright;
    }

    /// <summary>RGB5 additive brightness, saturating each component at 31 before packing.</summary>
    private static ushort Brighten(ushort color, int amount) =>
        (ushort)(Math.Min(31, (color & 31) + amount) |
            Math.Min(31, (color >> 5 & 31) + amount) << 5 |
            Math.Min(31, (color >> 10 & 31) + amount) << 10);
}
