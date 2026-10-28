using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Areas.Tourian.Artwork.TourianStatuePaintDefinitions;

namespace SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>
/// Paints the Tourian statue (<see cref="TourianStatuePaintDefinitions"/>): the base decoration's olive ramp and neutral shades, the
/// gold statue's glints and facets, the four boss-eye glow ramps and the released cool-stone greys, each calculated from
/// its selected anchor colours.
/// </summary>
internal static class TourianStatuePaint
{
    /// <summary>Calculates one native palette word from the selected statue paint domain and color index.</summary>
    internal static RgbColor Color(TourianStatuePaintBand band, int index) => band switch
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

    /// <summary>Interpolates an eight-bit stone channel and keeps its high five bits for CGRAM.</summary>
    private static int GreyChannel(int first, int last, int step) => (first * (6 - step) + last * step) / (6 * 8);
    /// <summary>Packs selected five-bit channel levels into a CGRAM color.</summary>
    private static RgbColor Pack(int r, int g, int b) => new RgbColor(r, g, b);
    /// <summary>Interpolates a channel with nearest-integer rounding.</summary>
    private static int Nearest(int first, int last, int step, int count) =>
        (first * (count - step) + last * step + count / 2) / count;
    /// <summary>Calculates the middle of a three-shade eye ramp using a geometric midpoint.</summary>
    private static int GammaShade(int first, int last, int step) =>
        step == 0 ? first : step == 2 ? last : (int)Math.Floor((first + last + 2 * Math.Sqrt(first * last)) / 4);

    /// <summary>Builds the base palette's olive ramp, shared eye colors, or neutral decorations.</summary>
    private static RgbColor Base(int color)
    {
        if (color == 0) return TransparentSlotPayload;
        if (color <= 8) return Pack(Nearest(BaseHighlightRed, 0, color - 1, 7),
            Nearest(Max, 0, color - 1, 7), Nearest(BaseHighlightBlue, 0, color - 1, 7));
        if (color <= 12) return Eye(1, color - 9);
        int neutral = NeutralHighlight - NeutralStep * (color - 13);
        return Pack(neutral, neutral, neutral);
    }

    /// <summary>Builds the gold statue's glints, facets, whites, and yellow accents.</summary>
    private static RgbColor Statue(int color)
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
        if (color == 15) return RgbColor.Black;
        int yellow = color == 12 ? Max : YellowAccent / (color - 12);
        return Pack(yellow, yellow, 0);
    }

    /// <summary>Builds one of the four boss-eye glow ramps from its tint and shade index.</summary>
    private static RgbColor Eye(int boss, int color)
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
