using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using static SuperMetroid.AssetExtraction.Bosses.Draygon.Artwork.DraygonHealthPaintDefinitions;
namespace SuperMetroid.AssetExtraction.Bosses.Draygon.Artwork;

/// <summary>
/// Four belly-material shades across eight health bands.
/// The health-palette update selects a band by health and copies its four colors
/// unchanged; hurt restoration copies the same shades.
/// </summary>
internal sealed class DraygonHealthPaint
{
    /// <summary>Extracted belly colors that differ from the interpolated gold-to-red ramp.</summary>
    internal readonly Dictionary<PaintColour, RgbColor> edits;

    /// <summary>Retains only differences from the four-shade, eight-band paint calculation.</summary>
    /// <param name="edits">The belly colours the import found differing from the calculated ramp.</param>
    internal DraygonHealthPaint(Dictionary<PaintColour, RgbColor> edits) => this.edits = edits;

    /// <summary>Uses an extracted edit when present, otherwise interpolates the requested belly shade.</summary>
    internal RgbColor Color(int band, int shade) => edits.TryGetValue(new PaintColour(band, shade), out RgbColor edited)
        ? edited : Calculate(band, shade);

    /// <summary>
    /// The critical endpoint is the selected pure-red quarter-shade
    /// ramp, nearest-rounded from RGB5 maximum. All intermediate channels use
    /// nearest sevenths; there are no retained output samples. The gold-to-red
    /// hue transition and shade assignment are selected health-color composition.
    /// </summary>
    internal static RgbColor Calculate(int band, int shade)
    {
        int healthyRed = HealthyRedStep * (Shades + 1 - shade);
        int healthyGreen = HealthyGreenStep * (Shades - shade);
        int criticalRed = (Maximum * (Shades - shade) + Shades / 2) / Shades;
        int red = Interpolate(healthyRed, criticalRed, band);
        int green = Interpolate(healthyGreen, 0, band);
        return new FadeLevelColour(red, green, 0).ToColour();
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
