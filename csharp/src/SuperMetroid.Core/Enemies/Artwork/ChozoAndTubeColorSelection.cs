using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The Chozo statues' and n00b-tube cracks' selected colours, kept as seeds and deviations from calculated paint.</summary>
/// <param name="TubeColorSeeds">Eight authored crack inks from which the unedited stock halves and shade ramp are calculated.</param>
/// <param name="TubeColorEdits">Supplied crack colors that differ from the stock seed and repeated ramp.</param>
/// <param name="WreckedShipEdits">Wrecked Ship statue inks that differ from the calculated Chozo paint.</param>
/// <param name="LowerNorfairEdits">Lower Norfair statue inks that differ from the calculated Chozo paint.</param>
internal sealed record ChozoAndTubeColorSelection(RgbColor[] TubeColorSeeds, Dictionary<ColourCell, RgbColor> TubeColorEdits,
    Dictionary<ColourCell, RgbColor> WreckedShipEdits, Dictionary<ColourCell, RgbColor> LowerNorfairEdits);
