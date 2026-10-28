using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Bosses.Draygon.Artwork.DraygonHealthPaintDefinitions;
namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// Four belly-material shades across eight health bands at $A5:96AF-96EE.
/// Native $A5:9701-9735 selects a band by health and copies its four colors
/// unchanged; hurt restoration at $A5:958C-959A copies the same shades.
/// </summary>
internal sealed class DraygonHealthPaint
{
    /// <summary>Extracted belly colors that differ from the interpolated gold-to-red ramp.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> edits = [];

    /// <summary>Retains only differences from the four-shade, eight-band paint calculation.</summary>
    internal DraygonHealthPaint(RgbColor[][] rows)
    {
        for (int band = 0; band < Bands; band++)
            for (int shade = 0; shade < Shades; shade++)
                if (rows[band][shade] != Calculate(band, shade)) edits.Add(new ColourCell(band, shade), rows[band][shade]);
    }

    /// <summary>Uses an extracted edit when present, otherwise interpolates the requested belly shade.</summary>
    internal RgbColor Color(int band, int shade) => edits.TryGetValue(new ColourCell(band, shade), out RgbColor edited)
        ? edited : Calculate(band, shade);

    /// <summary>
    /// The critical endpoint $A5:96E7-96EE is the selected pure-red quarter-shade
    /// ramp, nearest-rounded from RGB5 maximum. All intermediate channels use
    /// nearest sevenths; there are no retained output samples. The gold-to-red
    /// hue transition and shade assignment are selected health-color composition.
    /// </summary>
    private static RgbColor Calculate(int band, int shade)
    {
        int healthyRed = HealthyRedStep * (Shades + 1 - shade);
        int healthyGreen = HealthyGreenStep * (Shades - shade);
        int criticalRed = (Maximum * (Shades - shade) + Shades / 2) / Shades;
        int red = Interpolate(healthyRed, criticalRed, band);
        int green = Interpolate(healthyGreen, 0, band);
        return new RgbColor(red, green, 0);
    }

    /// <summary>Immutable healthy gold material shared with the primary body palette.</summary>
    internal static RgbColor HealthyShade(int shade)
    {
        if ((uint)shade >= Shades) throw new ArgumentOutOfRangeException(nameof(shade));
        return Calculate(0, shade);
    }

    /// <summary>Moves a color channel across seven health transitions with nearest-integer rounding.</summary>
    private static int Interpolate(int first, int last, int band)
    {
        int delta = last - first;
        return first + Math.Sign(delta) * ((Math.Abs(delta) * band + (Bands - 1) / 2) / (Bands - 1));
    }
}
