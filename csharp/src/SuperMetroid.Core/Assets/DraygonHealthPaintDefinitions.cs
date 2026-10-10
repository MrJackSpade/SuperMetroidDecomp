using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Four belly-material shades across eight health bands at $A5:96AF-96EE.
/// Native $A5:9701-9735 selects a band by health and copies its four colors
/// unchanged; hurt restoration at $A5:958C-959A copies the same shades.
/// </summary>
internal sealed class DraygonHealthPaintDefinitions
{
    private const int Shades = 4;
    private const int Bands = 8;
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
    private readonly Dictionary<int, Bgr555> edits = [];

    internal DraygonHealthPaintDefinitions(Bgr555[][] rows)
    {
        for (int band = 0; band < Bands; band++)
            for (int shade = 0; shade < Shades; shade++)
                if (rows[band][shade] != Calculate(band, shade)) edits.Add(band * Shades + shade, rows[band][shade]);
    }

    internal Bgr555 Color(int band, int shade) => edits.TryGetValue(band * Shades + shade, out Bgr555 edited)
        ? edited : Calculate(band, shade);

    /// <summary>
    /// The critical endpoint $A5:96E7-96EE is the selected pure-red quarter-shade
    /// ramp, nearest-rounded from RGB5 maximum. All intermediate channels use
    /// nearest sevenths; there are no retained output samples. The gold-to-red
    /// hue transition and shade assignment are selected health-color composition.
    /// </summary>
    private static Bgr555 Calculate(int band, int shade)
    {
        int healthyRed = HealthyRedStep * (Shades + 1 - shade);
        int healthyGreen = HealthyGreenStep * (Shades - shade);
        int criticalRed = (Maximum * (Shades - shade) + Shades / 2) / Shades;
        int red = Interpolate(healthyRed, criticalRed, band);
        int green = Interpolate(healthyGreen, 0, band);
        return new Bgr555(red, green, 0);
    }

    /// <summary>Immutable healthy gold material shared with the primary body palette.</summary>
    internal static Bgr555 HealthyShade(int shade)
    {
        if ((uint)shade >= Shades) throw new ArgumentOutOfRangeException(nameof(shade));
        return Calculate(0, shade);
    }

    private static int Interpolate(int first, int last, int band)
    {
        int delta = last - first;
        return first + Math.Sign(delta) * ((Math.Abs(delta) * band + (Bands - 1) / 2) / (Bands - 1));
    }
}
