using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.Botwoon;

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
    /// <param name="band">The health band, whose installed row is its place in <see cref="BotwoonHealthBandDefinitions.InOrderShown"/>.</param>
    /// <param name="color">Color index from zero through fifteen within the complete palette.</param>
    /// <returns>The installed colour.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The band is not one he shows, or the colour is outside its authored range.</exception>
    public RgbColor HealthColor(BotwoonHealthBand band, int color) => health.Colour(InstalledRows.Of(BotwoonHealthBandDefinitions.InOrderShown, band), color);
}
