using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Botwoon.Artwork;

/// <summary>Botwoon's installed health-band colours for its sprite palette, every band kept as installed.</summary>
public sealed class BotwoonColorCatalog
{
    /// <summary>The installed health-band colours selected by Botwoon's health.</summary>
    private readonly InstalledColourRows health;

    /// <summary>Keeps the complete installed health bands.</summary>
    /// <param name="health">Every health band's colours, as installed.</param>
    internal BotwoonColorCatalog(InstalledColourRows health) => this.health = health;

    /// <summary>Gets one installed colour of a health-band palette.</summary>
    /// <param name="band">Health-band index from zero through seven.</param>
    /// <param name="color">Color index from zero through fifteen within the complete palette.</param>
    /// <returns>The installed colour.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Either index is outside its authored range.</exception>
    public RgbColor HealthColor(int band, int color) => health.Colour(band, color);
}
