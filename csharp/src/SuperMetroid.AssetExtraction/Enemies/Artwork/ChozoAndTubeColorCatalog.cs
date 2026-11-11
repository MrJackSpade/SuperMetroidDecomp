using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// The installed RGB5 target images for the two Chozo statues and n00b-tube cracks, kept as installed.
/// Statue variant selection, PLM placement and tube behavior remain compiled.
/// </summary>
public sealed class ChozoAndTubeColorCatalog
{
    /// <summary>The installed tube-crack colours.</summary>
    private readonly InstalledColourRows tubeCracks;
    /// <summary>The installed Wrecked Ship statue colours.</summary>
    private readonly InstalledColourRows wreckedShip;
    /// <summary>The installed Lower Norfair statue colours.</summary>
    private readonly InstalledColourRows lowerNorfair;

    /// <summary>Keeps the complete installed images.</summary>
    /// <param name="tubeCracks">The tube cracks' colours.</param>
    /// <param name="wreckedShip">The Wrecked Ship statue's colours.</param>
    /// <param name="lowerNorfair">The Lower Norfair statue's colours.</param>
    /// <exception cref="InvalidDataException">An image is missing a colour.</exception>
    internal ChozoAndTubeColorCatalog(RgbColor[] tubeCracks, RgbColor[] wreckedShip, RgbColor[] lowerNorfair)
    {
        this.tubeCracks = new InstalledColourRows(tubeCracks, ChozoAndTubeColorDefinitions.ColorCount, "Chozo/tube tube cracks");
        this.wreckedShip = new InstalledColourRows(wreckedShip, ChozoAndTubeColorDefinitions.ColorCount, "Chozo/tube Wrecked Ship");
        this.lowerNorfair = new InstalledColourRows(lowerNorfair, ChozoAndTubeColorDefinitions.ColorCount, "Chozo/tube Lower Norfair");
    }

    /// <summary>Gets installed tube-crack ink 0–31.</summary>
    public RgbColor ResolveTubeCracks(int color) => tubeCracks.Colour(color);

    /// <summary>Gets installed Chozo statue ink 0–31 of the Wrecked Ship or Lower Norfair statue, which its shade shows over the room's two first enemy palettes.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The statue is undefined or the colour is outside it.</exception>
    internal RgbColor ResolveStatue(ChozoStatuePalette palette, int color) => palette switch
    {
        ChozoStatuePalette.WreckedShip => wreckedShip.Colour(color),
        ChozoStatuePalette.LowerNorfair => lowerNorfair.Colour(color),
        _ => throw new ArgumentOutOfRangeException(nameof(palette)),
    };
}
