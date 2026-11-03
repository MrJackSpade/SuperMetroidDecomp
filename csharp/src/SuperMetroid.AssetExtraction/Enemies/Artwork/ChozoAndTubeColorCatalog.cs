using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// Editable RGB5 target images for the two Chozo statues and n00b-tube cracks.
/// Statue variant selection, PLM placement and tube behavior remain compiled.
/// </summary>
public sealed class ChozoAndTubeColorCatalog
{
    /// <summary>The selected crack seeds and the tube and statue deviations from calculated paint.</summary>
    internal readonly ChozoAndTubeColorSelection selection;

    /// <summary>Preserves authored crack seeds and only deviations from calculated stock tube and statue colors.</summary>
    /// <param name="selection">The seeds and deviations the import selected.</param>
    internal ChozoAndTubeColorCatalog(ChozoAndTubeColorSelection selection) => this.selection = selection;

    /// <summary>Gets packed RGB5 tube-crack ink 0–31; the stock second sixteen-color half repeats the first, while supplied edits remain independent.</summary>
    public RgbColor ResolveTubeCracks(int color) => ChozoAndTubeColors.TubeCrack(this, color);

    /// <summary>Gets Chozo statue ink 0–31 of the Wrecked Ship or Lower Norfair statue, which its shade shows over the room's two first enemy palettes.</summary>
    internal RgbColor ResolveStatue(ChozoStatuePalette palette, int color) => ChozoAndTubeColors.Statue(this, palette, color);
}
