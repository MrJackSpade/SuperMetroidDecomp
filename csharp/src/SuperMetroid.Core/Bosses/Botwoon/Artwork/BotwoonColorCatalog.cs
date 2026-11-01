using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Botwoon.Artwork;

/// <summary>Editable health-band RGB5 images for Botwoon's sprite palette.</summary>
public sealed class BotwoonColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered health bands, independent of JSON encoding.</summary>
    public string ContentIdentity => BotwoonHealthColors.ContentIdentity(this);

    /// <summary>Compiled health-band colours selected by Botwoon's health.</summary>
    internal readonly BotwoonHealthPaint health;

    /// <summary>Retains the compiled health-band colours as runtime paint definitions.</summary>
    /// <param name="health">The health bands the import compiled.</param>
    internal BotwoonColorCatalog(BotwoonHealthPaint health) => this.health = health;

    /// <summary>Gets one compiled colour from an independently editable health-band palette.</summary>
    /// <param name="band">Health-band index from zero through seven.</param>
    /// <param name="color">Color index from zero through fifteen within the complete palette.</param>
    /// <returns>The selected colour.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Either index is outside its authored range.</exception>
    public RgbColor HealthColor(int band, int color) => BotwoonHealthColors.Read(health, band, color);
}
