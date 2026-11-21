using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of Tourian's glowing block and red-orb glow and the colours each writes, for the import.</summary>
/// <remarks>
/// The two effects share one run of records.
/// </remarks>
public static partial class TourianGlowPaletteFxColorDefinitions
{

    /// <summary>Eight live BGR555 colors are written by each record.</summary>
    internal const int ColorsPerFrame = 8;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(ColourEffectId.TourianGlowingArkanoidBlocksAndRedOrbs, ColorsPerFrame);
}
