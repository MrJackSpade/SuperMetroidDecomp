using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Game-over Baby color inputs with calculated cry-phase brightness steps.</summary>
/// <remarks>
/// Original $82:BD97..BE16 contains four sixteen-color phases. Native $82:BB9F..BBB4
/// copies a selected phase to OBJ palette 4; $82:CFF6/CFFD/D004 select the fixed
/// 16x16 Baby images at OBJ tiles $90/$92/$9B from $B6:C000. Their pixel inks are
/// categorical materials, not samples of a numeric independent variable.
///
/// Calculate the green five-level ramp, floor midpoints for organ/fang shades,
/// warm/cyan five-level cry tints and subsequent saturated three-level brightness.
/// MiddleCry holds fang red; glass shade 13 is capped below highlight 14.
/// ClosedCry glass shade preserves Idle green and shares it with blue; its red is
/// an independent endpoint. The green cry endpoint shares red/blue. The darkest
/// glass tint preserves Idle red. Import stores only independently differing
/// target channels, so source and target artwork edits remain exact without a cache.
///
/// The remaining stock artwork inputs are ten visible Idle endpoint colors:
/// green 4, organs 6/8, outline 9, fangs 10/12, and glass 1/13/14/15; plus the
/// ClosedCry green red=1, glass red=16 and outline red=4 choices. Those 33 RGB5
/// components choose the depicted materials' hues and endpoint intensities. Fixed
/// sprite pixels select them directly; animation supplies only phase/frame, not
/// geometry, illumination or a quantity defining those endpoint colors. Fitting
/// categorical ink numbers would re-encode the painting, the #1165 nonsense
/// exception. The actual shade/phase relationships are calculated separately.
///
/// Two further Idle words are preserved unused artwork metadata: ink 0 is skipped
/// by OBJ transparency, and ink 5 occurs in none of the three native Baby images.
/// Their original values cannot be deduced from drawn pixels; inventing a function
/// for this non-drawn metadata would likewise only encode the arbitrary payload.
/// Stock storage is therefore twelve base words, two endpoint red fields and one
/// differing outline-red component. This narrow disposition covers this palette
/// only; it does not exempt other palette families or their numerical relationships.
/// </remarks>
internal sealed class GameOverBabyColorCatalog
{
    private readonly Dictionary<int, ushort> inputs = new();
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> differences = new();
    private readonly int greenCryRed;
    private readonly int glassCryRed;

    /// <summary>Captures values from the four named palettes already validated by the presentation loader.</summary>
    internal GameOverBabyColorCatalog(IReadOnlyDictionary<string, ushort[]> palettes)
    {
        ushort[] closed = palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.ClosedCry)];
        greenCryRed = closed[4] & 31;
        glassCryRed = closed[13] & 31;
        for (int phase = 0; phase < 4; phase++)
        for (int color = 0; color < 16; color++)
        {
            ushort supplied = palettes[GameOverPresentationDefinitions.BabyPaletteName((GameOverBabyPalette)phase)][color];
            int key = phase * 16 + color;
            bool greenShade = phase < 2 && color is 2 or 3;
            bool warmCry = phase == 1 && color is >= 5 and <= 12;
            bool middleShade = phase == 0 && color is 7 or 11;
            bool coolCry = phase == 1 && color is 1 or 15;
            bool greenCry = phase == 1 && color == 4;
            bool glassHighlight = phase == 1 && color == 14;
            bool glassCry = phase == 1 && color == 13;
            if (!greenShade && !warmCry && !middleShade && !coolCry && !greenCry && !glassHighlight && !glassCry &&
                (phase == 0 || (phase == 1 && color != 0)))
            {
                inputs.Add(key, supplied);
                continue;
            }
            ushort expected = glassCry
                ? GlassCry(palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.Idle)][color])
                : glassHighlight
                ? Brighten(palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.Idle)][color], 5)
                : coolCry
                ? CoolCry(palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.Idle)][color], color)
                : greenCry ? GreenCry(palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.Idle)][color])
                : middleShade
                ? MiddleShade(palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.Idle)][color - 1],
                    palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.Idle)][color + 1])
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
        ushort expected = phase == 1 && color == 13
            ? GlassCry(Read(GameOverBabyPalette.Idle, color))
            : phase == 1 && color == 14
            ? Brighten(Read(GameOverBabyPalette.Idle, color), 5)
            : phase == 1 && color is 1 or 15
            ? CoolCry(Read(GameOverBabyPalette.Idle, color), color)
            : phase == 1 && color == 4 ? GreenCry(Read(GameOverBabyPalette.Idle, color))
            : phase == 0 && color is 7 or 11
            ? MiddleShade(Read(palette, color - 1), Read(palette, color + 1))
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

    private static ushort CoolCry(ushort idle, int ink) =>
        (ushort)((WarmCry(idle) & 0x7c00) | (ink == 15 ? idle & 31 : Math.Min(31, (idle & 31) + 5)) |
            Math.Min(31, (idle >> 5 & 31) + 5) << 5);

    private ushort GreenCry(ushort idle) =>
        (ushort)(greenCryRed | Math.Min(31, (idle >> 5 & 31) + 5) << 5 | greenCryRed << 10);

    private ushort GlassCry(ushort idle) =>
        (ushort)(glassCryRed | (idle & 0x03e0) | (idle >> 5 & 31) << 10);

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
