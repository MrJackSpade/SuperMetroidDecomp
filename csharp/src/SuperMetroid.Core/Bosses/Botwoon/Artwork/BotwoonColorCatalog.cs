using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Botwoon.Artwork;

/// <summary>Editable health-band RGB5 images for Botwoon's sprite palette.</summary>
public sealed class BotwoonColorCatalog
{
    /// <summary>Compiled health-band colours selected by Botwoon's health.</summary>
    internal readonly BotwoonHealthPaint health;

    /// <summary>Retains the validated health-band colours as runtime paint definitions.</summary>
    internal BotwoonColorCatalog(RgbColor[][] healthBands) => health = new(healthBands);

    /// <summary>Gets one compiled colour from an independently editable health-band palette.</summary>
    /// <param name="band">Health-band index from zero through seven.</param>
    /// <param name="color">Color index from zero through fifteen within the complete palette.</param>
    /// <returns>The selected colour.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Either index is outside its authored range.</exception>
    public RgbColor HealthColor(int band, int color) => BotwoonHealthColors.Read(health, band, color);
}
