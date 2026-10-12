using SuperMetroid.Core.Hardware;
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
    private readonly Dictionary<int, Bgr555> inputs = new();
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> differences = new();
    private readonly int greenCryRed;
    private readonly int glassCryRed;

    /// <summary>Captures values from the four named palettes already validated by the presentation loader.</summary>
    internal GameOverBabyColorCatalog(IReadOnlyDictionary<string, Bgr555[]> palettes)
    {
        Bgr555[] closed = palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.ClosedCry)];
        greenCryRed = closed[4].Red;
        glassCryRed = closed[13].Red;
        foreach (GameOverBabyPalette palette in Enum.GetValues<GameOverBabyPalette>())
        for (int color = 0; color < 16; color++)
        {
            Bgr555 supplied = palettes[GameOverPresentationDefinitions.BabyPaletteName(palette)][color];
            int key = (int)palette * 16 + color;
            bool idle = palette == GameOverBabyPalette.Idle;
            bool closedCry = palette == GameOverBabyPalette.ClosedCry;
            bool greenShade = (idle || closedCry) && color is 2 or 3;
            bool warmCry = closedCry && color is >= 5 and <= 12;
            bool middleShade = idle && color is 7 or 11;
            bool coolCry = closedCry && color is 1 or 15;
            bool greenCry = closedCry && color == 4;
            bool glassHighlight = closedCry && color == 14;
            bool glassCry = closedCry && color == 13;
            if (!greenShade && !warmCry && !middleShade && !coolCry && !greenCry && !glassHighlight && !glassCry &&
                (idle || (closedCry && color != 0)))
            {
                inputs.Add(key, supplied);
                continue;
            }
            Bgr555 expected = glassCry
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
                ? GreenShade(palettes[GameOverPresentationDefinitions.BabyPaletteName(palette)][4], color)
                : warmCry ? WarmCry(palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.Idle)][color])
                : color == 0
                ? palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.Idle)][0]
                : CryShade(palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalettes.PreviousCry(palette))][color],
                    palette, color, color == 13 ? palettes[GameOverPresentationDefinitions.BabyPaletteName(palette)][14] : Bgr555.Black);
            if (supplied != expected)
                differences.Add(key, new(supplied, expected));
        }
    }

    internal Bgr555 Read(GameOverBabyPalette palette, int color)
    {
        _ = GameOverPresentationDefinitions.BabyPaletteName(palette);
        if ((uint)color >= 16) throw new IndexOutOfRangeException();
        int key = (int)palette * 16 + color;
        if (inputs.TryGetValue(key, out Bgr555 value)) return value;
        bool idle = palette == GameOverBabyPalette.Idle;
        bool closedCry = palette == GameOverBabyPalette.ClosedCry;
        Bgr555 expected = closedCry && color == 13
            ? GlassCry(Read(GameOverBabyPalette.Idle, color))
            : closedCry && color == 14
            ? Brighten(Read(GameOverBabyPalette.Idle, color), 5)
            : closedCry && color is 1 or 15
            ? CoolCry(Read(GameOverBabyPalette.Idle, color), color)
            : closedCry && color == 4 ? GreenCry(Read(GameOverBabyPalette.Idle, color))
            : idle && color is 7 or 11
            ? MiddleShade(Read(palette, color - 1), Read(palette, color + 1))
            : (idle || closedCry) && color is 2 or 3
            ? GreenShade(Read(palette, 4), color)
            : closedCry && color is >= 5 and <= 12 ? WarmCry(Read(GameOverBabyPalette.Idle, color))
            : color == 0 ? inputs[0] : CryShade(Read(GameOverBabyPalettes.PreviousCry(palette), color),
                palette, color, color == 13 ? Read(palette, 14) : Bgr555.Black);
        return differences.TryGetValue(key, out var channels) ? channels.Apply(expected) : expected;
    }

    private static Bgr555 GreenShade(Bgr555 darkest, int color) =>
        darkest.WithGreen(Math.Min(31, darkest.Green + (4 - color) * 5));

    private static Bgr555 WarmCry(Bgr555 idle) =>
        new(Math.Min(31, idle.Red + 5), idle.Green, Math.Max(0, idle.Blue - 5));

    private static Bgr555 CoolCry(Bgr555 idle, int ink) =>
        new(ink == 15 ? idle.Red : Math.Min(31, idle.Red + 5), Math.Min(31, idle.Green + 5), WarmCry(idle).Blue);

    private Bgr555 GreenCry(Bgr555 idle) =>
        new(greenCryRed, Math.Min(31, idle.Green + 5), greenCryRed);

    private Bgr555 GlassCry(Bgr555 idle) =>
        new(glassCryRed, idle.Green, idle.Green);

    private static Bgr555 MiddleShade(Bgr555 bright, Bgr555 dark) =>
        bright.Zip(dark, (_, light, shadow) => (light + shadow) / 2);

    private static Bgr555 CryShade(Bgr555 previous, GameOverBabyPalette palette, int ink, Bgr555 highlight)
    {
        Bgr555 bright = Brighten(previous, 3);
        if (palette == GameOverBabyPalette.MiddleCry && ink is >= 10 and <= 12)
            return bright.WithRed(previous.Red);
        if (ink == 13)
            return bright.Zip(highlight, (_, brightened, glint) => Math.Min(brightened, Math.Max(0, glint - 1)));
        return bright;
    }

    /// <summary>RGB5 additive brightness, saturating each component at 31 before packing.</summary>
    private static Bgr555 Brighten(Bgr555 color, int amount) =>
        color.Map((_, channel) => Math.Min(31, channel + amount));
}
