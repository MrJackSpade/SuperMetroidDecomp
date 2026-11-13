using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The colours Samus's palette loaders read past their tables (#1500, #1512), decoded from the player's cartridge: import
/// data, which only the import reads to pre-colour her sheets (#1693). The installed overread document holds none of them.
/// </summary>
/// <param name="Version">The schema revision, <see cref="SamusOverreadFormat.ColourVersion"/>.</param>
/// <param name="SpeedBoosterOverrunColours">The colours Gravity copies on each Speed Booster step past its cycle, by step name.</param>
/// <param name="ChainsawBeamColours">The sixteen colours the Chainsaw beam's palette holds, in palette order.</param>
internal sealed record SamusOverrunColourDocument(
    int Version,
    IReadOnlyDictionary<string, IReadOnlyList<PaletteRgb5>> SpeedBoosterOverrunColours,
    IReadOnlyList<PaletteRgb5> ChainsawBeamColours);
