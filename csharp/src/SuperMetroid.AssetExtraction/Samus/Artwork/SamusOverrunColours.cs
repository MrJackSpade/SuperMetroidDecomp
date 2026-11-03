using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>
/// The colours two of Samus's palette loaders read past their tables (#1411, #1474), as the import decodes them from the
/// overread file (#1512). Only the import reads them: it pre-colours her sheets in the Gravity Speed Booster overrun shades
/// and the Chainsaw beam's shade from them, and gameplay names those shades.
/// </summary>
/// <param name="SpeedBooster">The suit colours Gravity Speed Booster copies from past its cycle.</param>
/// <param name="Chainsaw">The colours the Chainsaw beam's palette loader reads past its table.</param>
public sealed record SamusOverrunColours(SpeedBoosterOverrunColours<SpeedBoosterPaletteOverrun, RgbColor> SpeedBooster,
    ChainsawBeamColours<RgbColor> Chainsaw);
