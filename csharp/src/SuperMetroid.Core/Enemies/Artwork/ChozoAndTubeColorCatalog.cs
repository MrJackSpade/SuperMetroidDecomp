using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Editable RGB5 target images for the two Chozo statues and n00b-tube cracks.
/// Statue variant selection, PLM placement and tube behavior remain compiled.
/// </summary>
public sealed class ChozoAndTubeColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => ChozoAndTubeColors.ContentIdentity(this);

    /// <summary>The selected crack seeds and the tube and statue deviations from calculated paint.</summary>
    internal readonly ChozoAndTubeColorSelection selection;

    /// <summary>Preserves authored crack seeds and only deviations from calculated stock tube and statue colors.</summary>
    internal ChozoAndTubeColorCatalog(RgbColor[] tubeCracks, RgbColor[] wreckedShip,
        RgbColor[] lowerNorfair) =>
        selection = new(ChozoAndTubeColors.TubeSeeds(tubeCracks), ChozoAndTubeColors.TubeEdits(tubeCracks),
            ChozoAndTubeColors.StatueEdits(ChozoStatuePalette.WreckedShip, wreckedShip),
            ChozoAndTubeColors.StatueEdits(ChozoStatuePalette.LowerNorfair, lowerNorfair));

    /// <summary>Gets packed RGB5 tube-crack ink 0–31 corresponding to $AA:E2DD; the stock second sixteen-color half repeats the first, while supplied edits remain independent.</summary>
    public RgbColor ResolveTubeCracks(int color) => ChozoAndTubeColors.TubeCrack(this, color);

    /// <summary>Resolves a statue ink from an authored edit or the selected variant's calculated paint.</summary>
    internal RgbColor ResolveStatue(ChozoStatuePalette palette, int color) => ChozoAndTubeColors.Statue(this, palette, color);

    /// <summary>Installs the tube-crack image at CGRAM 144–175, matching palette-only initializer $AA:E716; tube destruction and the initializer actor's removal remain runtime behavior.</summary>
    public void ApplyTubeCracks(ColourRun destination) => ChozoAndTubeColors.ApplyTubeCracks(this, destination);
    /// <summary>Installs the 32 selected Wrecked Ship Chozo inks at CGRAM 144–175, replacing the $AA:E31D image copied by initializer $AA:E75C.</summary>
    public void ApplyWreckedShip(ColourRun destination) => ChozoAndTubeColors.ApplyStatue(this, destination, ChozoStatuePalette.WreckedShip);
    /// <summary>Installs the 32 selected Lower Norfair Chozo inks at CGRAM 144–175, replacing the $AA:E35D image copied by initializer $AA:E784.</summary>
    public void ApplyLowerNorfair(ColourRun destination) => ChozoAndTubeColors.ApplyStatue(this, destination, ChozoStatuePalette.LowerNorfair);
}
