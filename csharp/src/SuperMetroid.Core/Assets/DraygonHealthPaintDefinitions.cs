namespace SuperMetroid.Core.Assets;

/// <summary>
/// Four belly-material shades across eight health bands at $A5:96AF-96EE.
/// Native $A5:9701-9735 selects a band by health and copies its four colors
/// unchanged; hurt restoration at $A5:958C-959A copies the same shades.
/// </summary>
internal sealed class DraygonHealthPaintDefinitions
{
    /// <summary>Number of belly-material colors in each health band.</summary>
    private const int Shades = 4;
    /// <summary>Number of discrete health levels used to interpolate the belly palette.</summary>
    private const int Bands = 8;
    /// <summary>Maximum channel value in the five-bit RGB palette format.</summary>
    private const int Maximum = (1 << 5) - 1;
    /// <summary>
    /// $A5:96AF-96B6 (also normal palette slots9..12 at $A5:A289-A290):
    /// selected gold material steps red5/green6. Red leads green by one shade,
    /// producing red25..10 and green24..6; blue is zero. These material choices
    /// describe categorical belly paint. Replacing these gold-channel choices,
    /// the red-leading shade assignment or critical pure-red composition would
    /// invent different health-indicator artwork; the uniform copy supplies no
    /// rule selecting alternate paint. Pixels and health thresholds are separate.
    /// </summary>
    private const int HealthyRedStep = 5, HealthyGreenStep = 6;
    /// <summary>Sparse authored color replacements keyed by band and shade when they differ from the calculated palette.</summary>
    private readonly Dictionary<int, ushort> edits = [];

    /// <summary>Creates the health-paint lookup using authored row values only where they differ from the compiled interpolation.</summary>
    /// <param name="rows">Eight health bands, each containing the four RGB555 belly shades.</param>
    internal DraygonHealthPaintDefinitions(ushort[][] rows)
    {
        for (int band = 0; band < Bands; band++)
            for (int shade = 0; shade < Shades; shade++)
                if (rows[band][shade] != Calculate(band, shade)) edits.Add(band * Shades + shade, rows[band][shade]);
    }

    /// <summary>Gets a belly color from the sparse authored overrides or the interpolated health palette.</summary>
    /// <param name="band">Zero-based health band.</param>
    /// <param name="shade">Zero-based belly-material shade within the band.</param>
    /// <returns>The RGB555 color assigned to that band and shade.</returns>
    internal ushort Color(int band, int shade) => edits.TryGetValue(band * Shades + shade, out ushort edited)
        ? edited : Calculate(band, shade);

    /// <summary>
    /// The critical endpoint $A5:96E7-96EE is the selected pure-red quarter-shade
    /// ramp, nearest-rounded from RGB5 maximum. All intermediate channels use
    /// nearest sevenths; there are no retained output samples. The gold-to-red
    /// hue transition and shade assignment are selected health-color composition.
    /// </summary>
    private static ushort Calculate(int band, int shade)
    {
        int healthyRed = HealthyRedStep * (Shades + 1 - shade);
        int healthyGreen = HealthyGreenStep * (Shades - shade);
        int criticalRed = (Maximum * (Shades - shade) + Shades / 2) / Shades;
        int red = Interpolate(healthyRed, criticalRed, band);
        int green = Interpolate(healthyGreen, 0, band);
        return (ushort)(red | green << 5);
    }

    /// <summary>Immutable healthy gold material shared with the primary body palette.</summary>
    internal static ushort HealthyShade(int shade)
    {
        if ((uint)shade >= Shades) throw new ArgumentOutOfRangeException(nameof(shade));
        return Calculate(0, shade);
    }

    /// <summary>Linearly blends an RGB5 channel between endpoint values across the eight health bands using nearest-integer rounding.</summary>
    /// <param name="first">Channel value in the healthiest band.</param>
    /// <param name="last">Channel value in the critical-health band.</param>
    /// <param name="band">Zero-based position between the endpoint bands.</param>
    /// <returns>The rounded channel value for the selected band.</returns>
    private static int Interpolate(int first, int last, int band)
    {
        int delta = last - first;
        return first + Math.Sign(delta) * ((Math.Abs(delta) * band + (Bands - 1) / 2) / (Bands - 1));
    }
}
