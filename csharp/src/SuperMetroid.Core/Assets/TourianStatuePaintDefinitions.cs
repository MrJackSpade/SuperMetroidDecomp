using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Mutually exclusive native Tourian statue palette payload domains.</summary>
internal enum TourianStatuePaintBand { Base, Statue, Eye, Grey }

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
    private static readonly Bgr555 TransparentSlotPayload = Bgr555.FromWord(0x3800);
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

    internal static Bgr555 Color(TourianStatuePaintBand band, int index) => band switch
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

    private static int GreyChannel(int first, int last, int step) => (first * (6 - step) + last * step) / (6 * 8);
    private static Bgr555 Pack(int r, int g, int b) => new Bgr555(r, g, b);
    private static int Nearest(int first, int last, int step, int count) =>
        (first * (count - step) + last * step + count / 2) / count;
    private static int GammaShade(int first, int last, int step) =>
        step == 0 ? first : step == 2 ? last : (int)Math.Floor((first + last + 2 * Math.Sqrt(first * last)) / 4);

    private static Bgr555 Base(int color)
    {
        if (color == 0) return TransparentSlotPayload;
        if (color <= 8) return Pack(Nearest(BaseHighlightRed, 0, color - 1, 7),
            Nearest(Max, 0, color - 1, 7), Nearest(BaseHighlightBlue, 0, color - 1, 7));
        if (color <= 12) return Eye(1, color - 9);
        int neutral = NeutralHighlight - NeutralStep * (color - 13);
        return Pack(neutral, neutral, neutral);
    }

    private static Bgr555 Statue(int color)
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
        if (color == 15) return Bgr555.Black;
        int yellow = color == 12 ? Max : YellowAccent / (color - 12);
        return Pack(yellow, yellow, 0);
    }

    private static Bgr555 Eye(int boss, int color)
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