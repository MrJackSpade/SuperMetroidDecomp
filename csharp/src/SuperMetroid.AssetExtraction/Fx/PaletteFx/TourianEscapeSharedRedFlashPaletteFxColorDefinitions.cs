using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable mechanics for Tourian's shared general-level red-flash loop.</summary>
/// <remarks>
/// The two definitions select different colour destinations before
/// converging on one program. The 98 BGR555 words remain live presentation data; this
/// catalog owns entry routing, timing, the inline colour skip, waits, and loop control.
/// </remarks>
public static class TourianEscapeSharedRedFlashPaletteFxColorDefinitions
{

    /// <summary>Each record writes seven live BGR555 colors.</summary>
    public const int ColorsPerFrame = 7;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(ColourEffectAnimation.TourianRedFlashingArkanoidBlocksAndRedOrbs, ColorsPerFrame);
}
