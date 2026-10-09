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
    private const ushort EarlyBandTransparentTarget = 0x2003;

    /// <summary>Painted palette entries that differ from the deterministic health-ramp calculation.</summary>
    private readonly Dictionary<int, ushort> edits = [];

    /// <summary>Captures authored color exceptions while deriving the remaining entries from healthy and damaged endpoints.</summary>
    /// <param name="rows">Eight ordered health bands, each containing the sixteen copied palette colors.</param>
    internal BotwoonHealthPaintDefinitions(ushort[][] rows)
    {
        for (int band = 0; band < 8; band++) for (int color = 0; color < 16; color++)
            if (rows[band][color] != Calculate(band, color)) edits.Add(band * 16 + color, rows[band][color]);
    }

    /// <summary>Returns a retained painted value when present, otherwise computes the color for the requested health band.</summary>
    /// <param name="band">Zero-based position among the eight health palette bands.</param>
    /// <param name="color">Index of the color within the sixteen-entry palette row.</param>
    /// <returns>The authored override or calculated BGR555 color word.</returns>
    internal ushort Resolve(int band, int color) => edits.TryGetValue(band * 16 + color, out ushort value)
        ? value : Calculate(band, color);

    /// <summary>Interpolates a palette entry between healthy and damaged endpoints, preserving the separately copied transparent slot.</summary>
    /// <param name="band">Zero-based health-ramp row, from the healthy endpoint through the damaged endpoint.</param>
    /// <param name="color">Palette index whose packed BGR555 value is calculated.</param>
    /// <returns>The calculated color word for that band and palette slot.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="color"/> is not a supported palette index.</exception>
    private static ushort Calculate(int band, int color)
    {
        if (color == 0) return band is 1 or 2 ? EarlyBandTransparentTarget : (ushort)0;
        ushort first = Endpoint(false, color), last = Endpoint(true, color);
        int result = 0;
        for (int shift = 0; shift <= 10; shift += 5)
        {
            int start = first >> shift & 31, difference = (last >> shift & 31) - start;
            result |= (start + Math.Sign(difference) * ((Math.Abs(difference) * band + 3) / 7)) << shift;
        }
        return (ushort)result;
    }

    /// <summary>Selects an authored healthy or damaged endpoint color from the appropriate Botwoon artwork ramp.</summary>
    /// <param name="damaged"><see langword="true"/> for the damaged endpoint; otherwise selects the healthy endpoint.</param>
    /// <param name="color">Palette slot whose endpoint is being constructed.</param>
    /// <returns>The endpoint packed as a BGR555 word.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="color"/> is outside the copied palette row.</exception>
    private static ushort Endpoint(bool damaged, int color)
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
        return (ushort)(red | green << 5 | blue << 10);
    }

    /// <summary>Returns the authored RGB5 channel values for one shade of Botwoon's eye ramp.</summary>
    /// <param name="damaged"><see langword="true"/> selects the damaged eye palette.</param>
    /// <param name="shade">Offset within the four eye-color slots.</param>
    /// <returns>Red, green, and blue channel values used to pack the palette word.</returns>
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

    /// <summary>Builds one jaw shade from the healthy or damaged authored channel ramps.</summary>
    /// <param name="damaged"><see langword="true"/> selects the damaged jaw palette.</param>
    /// <param name="shade">Offset within the four jaw-color slots.</param>
    /// <returns>Red, green, and blue RGB5 channel values for the selected shade.</returns>
    private static (int Red, int Green, int Blue) Jaw(bool damaged, int shade)
    {
        static int Ramp(int peak, int floor, int phase) => (peak * (3 - phase) + floor * phase) / 3;
        if (damaged) return (Ramp(30, 23, shade), Math.Max(0, 12 - 9 * shade / 2), Ramp(10, 5, shade));
        int blue = Ramp(15, 4, shade), red = blue + 13 - shade;
        int green = shade switch { 0 => red, 1 => 20, 2 => 11, _ => blue };
        return (red, green, blue);
    }

    /// <summary>Builds one skin shade while retaining the distinct healthy and damaged red/green progression.</summary>
    /// <param name="damaged"><see langword="true"/> selects the damaged skin palette.</param>
    /// <param name="shade">Offset within the five skin-color slots.</param>
    /// <returns>Red, green, and blue RGB5 channel values for the selected shade.</returns>
    private static (int Red, int Green, int Blue) Skin(bool damaged, int shade)
    {
        int healthyGreen = shade == 4 ? 1 : 18 - 4 * shade;
        if (!damaged) return (shade == 4 ? 12 : 31 - 9 * shade / 2, healthyGreen, 2);
        int red = shade switch { 0 => 31, 1 => 28, 2 => 23, 3 => 20, _ => 15 };
        return (red, Math.Max(1, healthyGreen / 2), 6);
    }
}
