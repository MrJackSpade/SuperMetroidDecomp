using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Areas.Tourian.Artwork;

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
    internal static readonly RgbColor TransparentSlotPayload = new(0, 0, 14);
    /// <summary>Maximum value of a five-bit RGB color channel.</summary>
    internal const int Max = 31;
    /// <summary>$AA:D787: olive base highlight RGB(25,31,9), fading to black at color8.</summary>
    internal const int BaseHighlightRed = 25, BaseHighlightBlue = 9;
    /// <summary>$AA:D79F..D7A4: neutral splash/decoration shades20,14,8.</summary>
    internal const int NeutralHighlight = 20, NeutralStep = 6;
    /// <summary>$AA:D767/69: gold glint blue21 then half-intensity blue, with saturated R/G.</summary>
    internal const int GoldGlintBlue = 21;
    /// <summary>$AA:D76B: selected bright gold facet RGB(28,25,7).</summary>
    internal const int GoldFacetRed = 28, GoldFacetGreen = 25, GoldFacetBlue = 7;
    /// <summary>$AA:D76D: selected middle gold facet RGB(24,19,0).</summary>
    internal const int GoldMiddleRed = 24, GoldMiddleGreen = 19;
    /// <summary>$AA:D76F..75: lower red intensities are1 plus triangular numbers5..2.
    /// Their green shade is red minus3, clipped to the selected floor2.</summary>
    internal const int GoldDarkLevel = 5, GoldDarkFloor = 1, GoldGreenDrop = 3, GoldGreenFloor = 2;
    /// <summary>$AA:D77F/81: equal R/G yellow shade18 followed by its exact half.</summary>
    internal const int YellowAccent = 18;
    /// <summary>$86:B91E/26/2E/36: each tinted eye glint mutes selected channels to26.</summary>
    internal const int GlintTint = 26;
    /// <summary>$86:B920..24: yellow eye R27..19 uses floor gamma2; G25..11 follows its red-dependent hue.</summary>
    internal const int YellowRedLight = 27, YellowRedDark = 19, YellowGreenLight = 25, YellowGreenDark = 11;
    /// <summary>$86:B928..2C: equal R/B magenta shades interpolate31..12 with nearest rounding.</summary>
    internal const int MagentaDark = 12;
    /// <summary>$86:B930..34: blue eye green22..7 is linear-nearest; blue follows2/3 green plus its saturated-light anchor.</summary>
    internal const int BlueGreenLight = 22, BlueGreenDark = 7;
    /// <summary>$86:B938..3C: green eye shades31..12 use floor gamma2; blue is floor(14*green/31).</summary>
    internal const int GreenDark = 12, GreenBlueLight = 14;
    /// <summary>$87:839E..83AA: representative RGB8 light(199,214,255) and dark(31,32,39)
    /// paints calculate every RGB5 word by linear interpolation then high-five-bit quantization.
    /// The source samples do not uniquely recover an RGB8 original. These are an exact chosen
    /// representation, not a claim about the original authoring tool or hidden source colors.</summary>
    internal const int GreyLightRed = 199, GreyLightGreen = 214, GreyLightBlue = 255,
        GreyDarkRed = 31, GreyDarkGreen = 32, GreyDarkBlue = 39;
}
