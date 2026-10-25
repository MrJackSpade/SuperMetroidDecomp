using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.Botwoon.Artwork;

/// <summary>
/// Botwoon health colors $B3:971B-981A, copied uniformly by $B3:9850-9868.
/// Healthy endpoint $B3:971B is also the header palette at $B3:9319; that separate
/// installed occurrence retains its own inventory obligation. Material endpoint
/// magnitudes and ramp policies specify the reviewed eye/jaw/skin/glint/outline
/// artwork. Regenerating those choices without the paint inputs invents different
/// content. Copied slot0 metadata is separately preserved, not inferred from visibility.
/// No health thresholds, sprite pixels or other palette entries are exempted.
/// </summary>
internal sealed class BotwoonHealthPaintDefinitions
{
    /// <summary>$B3:973B/975B: copied slot0 target in bands1/2; independently observable CGRAM data.</summary>
    private static readonly RgbColor EarlyBandTransparentTarget = RgbColor.FromWord(0x2003);
    /// <summary>Imported band colors that differ from the reviewed endpoint interpolation.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> edits = [];

    /// <summary>Retains source colors only where they differ from the eight-band health ramp.</summary>
    internal BotwoonHealthPaintDefinitions(RgbColor[][] rows)
    {
        for (int band = 0; band < 8; band++) for (int color = 0; color < 16; color++)
            if (rows[band][color] != Calculate(band, color)) edits.Add(new ColourCell(band, color), rows[band][color]);
    }

    /// <summary>Returns an imported exception or interpolates the selected healthy and damaged paints.</summary>
    internal RgbColor Resolve(int band, int color) => edits.TryGetValue(new ColourCell(band, color), out RgbColor value)
        ? value : Calculate(band, color);

    /// <summary>Interpolates RGB5 channels over seven intervals while preserving copied slot-zero metadata.</summary>
    private static RgbColor Calculate(int band, int color)
    {
        if (color == 0) return band is 1 or 2 ? EarlyBandTransparentTarget : RgbColor.Black;
        RgbColor first = Endpoint(BotwoonHealthEndpoint.Healthy, color), last = Endpoint(BotwoonHealthEndpoint.Damaged, color);
        return first.Zip(last, (_, start, end) =>
        {
            int difference = end - start;
            return start + Math.Sign(difference) * ((Math.Abs(difference) * band + 3) / 7);
        });
    }

    /// <summary>Chooses the healthy or damaged material paint for one Botwoon palette slot.</summary>
    private static RgbColor Endpoint(BotwoonHealthEndpoint endpoint, int color)
    {
        return color switch
        {
            >= 1 and <= 4 => Eye(endpoint == BotwoonHealthEndpoint.Damaged, color - 1),
            >= 5 and <= 8 => Jaw(endpoint == BotwoonHealthEndpoint.Damaged, color - 5),
            >= 9 and <= 13 => Skin(endpoint == BotwoonHealthEndpoint.Damaged, color - 9),
            14 => endpoint == BotwoonHealthEndpoint.Damaged ? new RgbColor(31, 31, 17) : new RgbColor(29, 29, 31),
            15 => endpoint == BotwoonHealthEndpoint.Damaged ? new RgbColor(11, 0, 0) : new RgbColor(5, 0, 3),
            _ => throw new ArgumentOutOfRangeException(nameof(color)),
        };
    }

    /// <summary>Supplies four eye shades, shifting from gray-green highlights to warmer damaged colors.</summary>
    private static RgbColor Eye(bool damaged, int shade)
    {
        if (!damaged)
        {
            int grey = shade switch { 0 => 9, 1 => 6, 2 => 5, _ => 3 };
            return new RgbColor(grey, shade == 0 ? 31 : 19 - 7 * (shade - 1), grey);
        }
        int red = shade switch { 0 => 24, 1 => 21, 2 => 17, _ => 11 };
        int green = shade switch { 0 => 31, 1 => 24, 2 => 18, _ => red };
        int blue = shade switch { 0 => 7, 3 => 1, _ => 3 };
        return new RgbColor(red, green, blue);
    }

    /// <summary>Builds four jaw shades from healthy blue and damaged red ramps.</summary>
    private static RgbColor Jaw(bool damaged, int shade)
    {
        static int Ramp(int peak, int floor, int phase) => (peak * (3 - phase) + floor * phase) / 3;
        if (damaged) return new RgbColor(Ramp(30, 23, shade), Math.Max(0, 12 - 9 * shade / 2), Ramp(10, 5, shade));
        int blue = Ramp(15, 4, shade), red = blue + 13 - shade;
        int green = shade switch { 0 => red, 1 => 20, 2 => 11, _ => blue };
        return new RgbColor(red, green, blue);
    }

    /// <summary>Builds five skin shades with reduced green in the damaged endpoint.</summary>
    private static RgbColor Skin(bool damaged, int shade)
    {
        int healthyGreen = shade == 4 ? 1 : 18 - 4 * shade;
        if (!damaged) return new RgbColor(shade == 4 ? 12 : 31 - 9 * shade / 2, healthyGreen, 2);
        int red = shade switch { 0 => 31, 1 => 28, 2 => 23, 3 => 20, _ => 15 };
        return new RgbColor(red, Math.Max(1, healthyGreen / 2), 6);
    }
}
