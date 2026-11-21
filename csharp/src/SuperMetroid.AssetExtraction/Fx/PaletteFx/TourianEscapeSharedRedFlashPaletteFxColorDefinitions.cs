using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of Tourian's shared red flash and the colours each writes, for the import.</summary>
/// <remarks>
/// The two effects write different colours of the screen but share one run of records.
/// </remarks>
public static class TourianEscapeSharedRedFlashPaletteFxColorDefinitions
{

    /// <summary>Each record writes seven live BGR555 colors.</summary>
    internal const int ColorsPerFrame = 7;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(ColourEffectId.TourianRedFlashingArkanoidBlocksAndRedOrbs, ColorsPerFrame);
}
