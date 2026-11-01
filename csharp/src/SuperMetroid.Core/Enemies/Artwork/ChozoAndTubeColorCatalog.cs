using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Editable RGB5 target images for the two Chozo statues and n00b-tube cracks.
/// Statue variant selection, PLM placement and tube behavior remain compiled.
/// </summary>
public sealed class ChozoAndTubeColorCatalog
{
    /// <summary>Canonical selected RGB5 colors in their tube and statue order, independent of JSON encoding.</summary>
    public string ContentIdentity => ChozoAndTubeColors.ContentIdentity(this);

    /// <summary>The selected crack seeds and the tube and statue deviations from calculated paint.</summary>
    internal readonly ChozoAndTubeColorSelection selection;

    /// <summary>Preserves authored crack seeds and only deviations from calculated stock tube and statue colors.</summary>
    internal ChozoAndTubeColorCatalog(RgbColor[] tubeCracks, RgbColor[] wreckedShip,
        RgbColor[] lowerNorfair) =>
        selection = new(ChozoAndTubeColors.TubeSeeds(tubeCracks), ChozoAndTubeColors.TubeEdits(tubeCracks),
            ChozoAndTubeColors.StatueEdits(ChozoStatuePalette.WreckedShip, wreckedShip),
            ChozoAndTubeColors.StatueEdits(ChozoStatuePalette.LowerNorfair, lowerNorfair));

    /// <summary>Gets packed RGB5 tube-crack ink 0–31; the stock second sixteen-color half repeats the first, while supplied edits remain independent.</summary>
    public RgbColor ResolveTubeCracks(int color) => ChozoAndTubeColors.TubeCrack(this, color);

    /// <summary>Installs the tube-crack image at palette colours 144–175, matching the palette-only tube-crack initializer; tube destruction and the initializer actor's removal remain runtime behavior.</summary>
    public void ApplyTubeCracks(ColourRunSequence destination) => ChozoAndTubeColors.ApplyTubeCracks(this, destination);
    /// <summary>Installs the 32 selected Wrecked Ship Chozo inks at palette colours 144–175, replacing the stock image the Wrecked Ship statue initializer copies.</summary>
    public void ApplyWreckedShip(ColourRunSequence destination) => ChozoAndTubeColors.ApplyStatue(this, destination, ChozoStatuePalette.WreckedShip);
    /// <summary>Installs the 32 selected Lower Norfair Chozo inks at palette colours 144–175, replacing the stock image the Lower Norfair statue initializer copies.</summary>
    public void ApplyLowerNorfair(ColourRunSequence destination) => ChozoAndTubeColors.ApplyStatue(this, destination, ChozoStatuePalette.LowerNorfair);
}
