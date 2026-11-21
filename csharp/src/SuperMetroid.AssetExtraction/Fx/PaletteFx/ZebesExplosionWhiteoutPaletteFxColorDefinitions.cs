using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The records of the shared wide-background and space whiteout and the colours each writes, for the import.
/// </summary>
/// <remarks>
/// The two effects write their own colours through one shared run of records, which brighten from black to white;
/// this describes the authored colours, not arithmetic the game performs.
/// </remarks>
public static partial class ZebesExplosionWhiteoutPaletteFxColorDefinitions
{

    /// <summary>Each record writes one live BGR555 color.</summary>
    internal const int ColorsPerFrame = 1;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(ColourEffectId.WhiteOutSpaceDuringZebesExplosion, ColorsPerFrame);
}
