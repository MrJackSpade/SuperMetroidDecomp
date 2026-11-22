using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the Zebes explosion finale and the colours each writes, for the import.</summary>
/// <remarks>
/// The finale's records slide a white-to-black gradient in, rotate it and slide white back; this describes the authored
/// colours, not arithmetic the game performs.
/// </remarks>
public static partial class ZebesExplosionFinalePaletteFxColorDefinitions
{

    /// <summary>Each record writes fifteen live BGR555 colors.</summary>
    internal const int ColorsPerFrame = 15;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(ColourEffectId.ZebesExplosionFinale, ColorsPerFrame);
}
