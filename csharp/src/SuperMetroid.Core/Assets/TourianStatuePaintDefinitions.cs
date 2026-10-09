namespace SuperMetroid.Core.Assets;

/// <summary>Mutually exclusive native Tourian statue palette payload domains.</summary>
internal enum TourianStatuePaintBand
{
    /// <summary>Base decoration inks copied into the statue's OBJ palette band.</summary>
    Base,
    /// <summary>Gold statue body inks used by the entrance presentation.</summary>
    Statue,
    /// <summary>Four boss-specific eye-glow rows installed over the base decoration.</summary>
    Eye,
    /// <summary>Grey-transition inks consumed by the statue's release animation.</summary>
    Grey
}

/// <summary>
/// Selected decoration, gold statue, boss-eye and released cool-stone paints.
/// Native $AA:D765/D785, $86:B91E and $87:839C consumers copy these colors to
/// palettes; none interprets a color as motion, health, collision or duration.
/// Shared shades calculate. Exact remaining material/hue/rounding choices define
/// the illustrated statue and glow, not a hidden gameplay function.
/// </summary>
internal static class TourianStatuePaintDefinitions
{
    /// <summary>Native copied transparent-slot word at $AA:D765/D785 and $87:839C.</summary>
    private const ushort TransparentSlotPayload = 0x3800;
    /// <summary>Largest representable channel value in the five-bit SNES color format.</summary>
    private const int Max = 31;
    /// <summary>$AA:D787: olive base highlight RGB(25,31,9), fading to black at color8.</summary>
    private const int BaseHighlightRed = 25, BaseHighlightBlue = 9;
    /// <summary>$AA:D79F..D7A4: neutral splash/decoration shades20,14,8.</summary>
    private const int NeutralHighlight = 20, NeutralStep = 6;
    /// <summary>$AA:D767/69: gold glint blue21 then half-intensity blue, with saturated R/G.</summary>
    private const int GoldGlintBlue = 21;
    /// <summary>$AA:D76B: selected bright gold facet RGB(28,25,7).</summary>
    private const int GoldFacetRed = 28, GoldFacetGreen = 25, GoldFacetBlue = 7;
    /// <summary>$AA:D76D: selected middle gold facet RGB(24,19,0).</summary>
    private const int GoldMiddleRed = 24, GoldMiddleGreen = 19;
    /// <summary>$AA:D76F..75: lower red intensities are1 plus triangular numbers5..2.
    /// Their green shade is red minus3, clipped to the selected floor2.</summary>
    private const int GoldDarkLevel = 5, GoldDarkFloor = 1, GoldGreenDrop = 3, GoldGreenFloor = 2;
    /// <summary>$AA:D77F/81: equal R/G yellow shade18 followed by its exact half.</summary>
    private const int YellowAccent = 18;
    /// <summary>$86:B91E/26/2E/36: each tinted eye glint mutes selected channels to26.</summary>
    private const int GlintTint = 26;
    /// <summary>$86:B920..24: yellow eye R27..19 uses floor gamma2; G25..11 follows its red-dependent hue.</summary>
    private const int YellowRedLight = 27, YellowRedDark = 19, YellowGreenLight = 25, YellowGreenDark = 11;
    /// <summary>$86:B928..2C: equal R/B magenta shades interpolate31..12 with nearest rounding.</summary>
    private const int MagentaDark = 12;
    /// <summary>$86:B930..34: blue eye green22..7 is linear-nearest; blue follows2/3 green plus its saturated-light anchor.</summary>
    private const int BlueGreenLight = 22, BlueGreenDark = 7;
    /// <summary>$86:B938..3C: green eye shades31..12 use floor gamma2; blue is floor(14*green/31).</summary>
    private const int GreenDark = 12, GreenBlueLight = 14;
    /// <summary>$87:839E..83AA: representative RGB8 light(199,214,255) and dark(31,32,39)
    /// paints calculate every RGB5 word by linear interpolation then high-five-bit quantization.
    /// The source samples do not uniquely recover an RGB8 original. These are an exact chosen
    /// representation, not a claim about the original authoring tool or hidden source colors.</summary>
    private const int GreyLightRed = 199, GreyLightGreen = 214, GreyLightBlue = 255,
        GreyDarkRed = 31, GreyDarkGreen = 32, GreyDarkBlue = 39;

    /// <summary>Calculates the packed RGB5 word for one slot in a native palette domain.</summary>
    /// <param name="band">Palette domain whose stock paint rules are applied.</param>
    /// <param name="index">Zero-based slot within that domain; eye slots are grouped four per boss row.</param>
    /// <returns>The SNES color word selected by the domain's paint definition.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The palette domain is not recognized.</exception>
    internal static ushort Color(TourianStatuePaintBand band, int index) => band switch
    {
        TourianStatuePaintBand.Base => Base(index),
        TourianStatuePaintBand.Statue => Statue(index),
        TourianStatuePaintBand.Eye => Eye(index / 4, index % 4),
        TourianStatuePaintBand.Grey => index == 0 ? TransparentSlotPayload :
            Pack(GreyChannel(GreyLightRed, GreyDarkRed, index - 1),
                GreyChannel(GreyLightGreen, GreyDarkGreen, index - 1),
                GreyChannel(GreyLightBlue, GreyDarkBlue, index - 1)),
        _ => throw new ArgumentOutOfRangeException(nameof(band)),
    };

    /// <summary>Interpolates an RGB8 channel between endpoint samples and quantizes it to RGB5.</summary>
    /// <param name="first">RGB8 channel value at the light end of the transition.</param>
    /// <param name="last">RGB8 channel value at the dark end of the transition.</param>
    /// <param name="step">Zero-based transition step between the endpoint samples.</param>
    /// <returns>The quantized five-bit channel value.</returns>
    private static int GreyChannel(int first, int last, int step) => (first * (6 - step) + last * step) / (6 * 8);

    /// <summary>Packs five-bit red, green, and blue channels into a SNES color word.</summary>
    /// <param name="r">Five-bit red channel.</param>
    /// <param name="g">Five-bit green channel.</param>
    /// <param name="b">Five-bit blue channel.</param>
    /// <returns>The packed BGR555-format color word.</returns>
    private static ushort Pack(int r, int g, int b) => (ushort)(r | g << 5 | b << 10);

    /// <summary>Linearly interpolates between channel endpoints with nearest-integer rounding.</summary>
    /// <param name="first">Channel value at the beginning of the ramp.</param>
    /// <param name="last">Channel value at the end of the ramp.</param>
    /// <param name="step">Zero-based position within the ramp.</param>
    /// <param name="count">Number of intervals between its endpoints.</param>
    /// <returns>The rounded channel value at the requested position.</returns>
    private static int Nearest(int first, int last, int step, int count) =>
        (first * (count - step) + last * step + count / 2) / count;

    /// <summary>Interpolates channel endpoints using the catalog's square-root gamma curve.</summary>
    /// <param name="first">Channel value at the light end of the curve.</param>
    /// <param name="last">Channel value at the dark end of the curve.</param>
    /// <param name="step">Zero-based position in the three-sample shading curve.</param>
    /// <returns>The floored channel value for this gamma-shaded step.</returns>
    private static int GammaShade(int first, int last, int step) =>
        step == 0 ? first : step == 2 ? last : (int)Math.Floor((first + last + 2 * Math.Sqrt(first * last)) / 4);

    /// <summary>Calculates a base-decoration slot, combining the olive fade, Ridley eye colors, and neutral accents.</summary>
    /// <param name="color">Zero-based slot in the sixteen-color base palette.</param>
    /// <returns>The packed RGB5 word for that slot.</returns>
    private static ushort Base(int color)
    {
        if (color == 0) return TransparentSlotPayload;
        if (color <= 8) return Pack(Nearest(BaseHighlightRed, 0, color - 1, 7),
            Nearest(Max, 0, color - 1, 7), Nearest(BaseHighlightBlue, 0, color - 1, 7));
        if (color <= 12) return Eye(1, color - 9);
        int neutral = NeutralHighlight - NeutralStep * (color - 13);
        return Pack(neutral, neutral, neutral);
    }

    /// <summary>Calculates the statue-body palette, including its gold facets, shaded lower body, and yellow accent.</summary>
    /// <param name="color">Zero-based slot in the sixteen-color statue palette.</param>
    /// <returns>The packed RGB5 word for that slot.</returns>
    private static ushort Statue(int color)
    {
        if (color == 0) return TransparentSlotPayload;
        if (color is 1 or 2) return Pack(Max, Max, GoldGlintBlue / color);
        if (color == 3) return Pack(GoldFacetRed, GoldFacetGreen, GoldFacetBlue);
        if (color == 4) return Pack(GoldMiddleRed, GoldMiddleGreen, 0);
        if (color <= 8)
        {
            int level = GoldDarkLevel - (color - 5);
            int red = GoldDarkFloor + level * (level + 1) / 2;
            return Pack(red, Math.Max(GoldGreenFloor, red - GoldGreenDrop), 0);
        }
        if (color <= 11) return Pack(Max, Max, Max);
        if (color == 15) return 0;
        int yellow = color == 12 ? Max : YellowAccent / (color - 12);
        return Pack(yellow, yellow, 0);
    }

    /// <summary>Calculates one boss's four-color eye row using its selected glint and shading ramp.</summary>
    /// <param name="boss">Zero-based boss row: Phantoon, Ridley, Draygon, or Kraid.</param>
    /// <param name="color">Zero-based slot within that four-color row.</param>
    /// <returns>The packed RGB5 word for the requested eye color.</returns>
    private static ushort Eye(int boss, int color)
    {
        if (color == 0) return boss switch
        {
            0 => Pack(Max, Max, GlintTint),
            1 => Pack(Max, GlintTint, Max),
            2 => Pack(GlintTint, GlintTint, Max),
            3 => Pack(GlintTint, Max, GlintTint),
            _ => throw new ArgumentOutOfRangeException(nameof(boss)),
        };
        int step = color - 1;
        switch (boss)
        {
            case 0:
                int red = GammaShade(YellowRedLight, YellowRedDark, step);
                int green = YellowGreenDark + (red - YellowRedDark) *
                    (YellowGreenLight - YellowGreenDark) / (YellowRedLight - YellowRedDark);
                return Pack(red, green, 0);
            case 1:
                int magenta = Nearest(Max, MagentaDark, step, 2);
                return Pack(magenta, 0, magenta);
            case 2:
                int blueGreen = Nearest(BlueGreenLight, BlueGreenDark, step, 2);
                return Pack(0, blueGreen, Max - BlueGreenLight * 2 / 3 + blueGreen * 2 / 3);
            case 3:
                int brightGreen = GammaShade(Max, GreenDark, step);
                return Pack(0, brightGreen, brightGreen * GreenBlueLight / Max);
            default: throw new ArgumentOutOfRangeException(nameof(boss));
        }
    }
}
