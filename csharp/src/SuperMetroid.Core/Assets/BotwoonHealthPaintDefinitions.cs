using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

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
    private static readonly Bgr555 EarlyBandTransparentTarget = Bgr555.FromWord(0x2003);
    private readonly Dictionary<int, Bgr555> edits = [];

    internal BotwoonHealthPaintDefinitions(Bgr555[][] rows)
    {
        for (int band = 0; band < 8; band++) for (int color = 0; color < 16; color++)
            if (rows[band][color] != Calculate(band, color)) edits.Add(band * 16 + color, rows[band][color]);
    }

    internal Bgr555 Resolve(int band, int color) => edits.TryGetValue(band * 16 + color, out Bgr555 value)
        ? value : Calculate(band, color);

    private static Bgr555 Calculate(int band, int color)
    {
        if (color == 0) return band is 1 or 2 ? EarlyBandTransparentTarget : Bgr555.Black;
        Bgr555 first = Endpoint(false, color), last = Endpoint(true, color);
        return first.Zip(last, (_, start, end) =>
        {
            int difference = end - start;
            return start + Math.Sign(difference) * ((Math.Abs(difference) * band + 3) / 7);
        });
    }

    private static Bgr555 Endpoint(bool damaged, int color)
    {
        (int red, int green, int blue) = color switch
        {
            >= 1 and <= 4 => Eye(damaged, color - 1),
            >= 5 and <= 8 => Jaw(damaged, color - 5),
            >= 9 and <= 13 => Skin(damaged, color - 9),
            14 => damaged ? (31, 31, 17) : (29, 29, 31),
            15 => damaged ? (11, 0, 0) : (5, 0, 3),
            _ => throw new ArgumentOutOfRangeException(nameof(color)),
        };
        return new Bgr555(red, green, blue);
    }

    private static (int Red, int Green, int Blue) Eye(bool damaged, int shade)
    {
        if (!damaged)
        {
            int grey = shade switch { 0 => 9, 1 => 6, 2 => 5, _ => 3 };
            return (grey, shade == 0 ? 31 : 19 - 7 * (shade - 1), grey);
        }
        int red = shade switch { 0 => 24, 1 => 21, 2 => 17, _ => 11 };
        int green = shade switch { 0 => 31, 1 => 24, 2 => 18, _ => red };
        int blue = shade switch { 0 => 7, 3 => 1, _ => 3 };
        return (red, green, blue);
    }

    private static (int Red, int Green, int Blue) Jaw(bool damaged, int shade)
    {
        static int Ramp(int peak, int floor, int phase) => (peak * (3 - phase) + floor * phase) / 3;
        if (damaged) return (Ramp(30, 23, shade), Math.Max(0, 12 - 9 * shade / 2), Ramp(10, 5, shade));
        int blue = Ramp(15, 4, shade), red = blue + 13 - shade;
        int green = shade switch { 0 => red, 1 => 20, 2 => 11, _ => blue };
        return (red, green, blue);
    }

    private static (int Red, int Green, int Blue) Skin(bool damaged, int shade)
    {
        int healthyGreen = shade == 4 ? 1 : 18 - 4 * shade;
        if (!damaged) return (shade == 4 ? 12 : 31 - 9 * shade / 2, healthyGreen, 2);
        int red = shade switch { 0 => 31, 1 => 28, 2 => 23, 3 => 20, _ => 15 };
        return (red, Math.Max(1, healthyGreen / 2), 6);
    }
}
