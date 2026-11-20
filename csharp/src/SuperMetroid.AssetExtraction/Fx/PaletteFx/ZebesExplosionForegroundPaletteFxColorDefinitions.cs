using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the Zebes explosion's wide foreground and the colours each writes, for the import.</summary>
/// <remarks>
/// Each record lights one more of the foreground's colours than the record before, the rest black.
/// </remarks>
public static class ZebesExplosionForegroundPaletteFxColorDefinitions
{

    /// <summary>Each record writes fifteen live BGR555 colors.</summary>
    private const int ColorsPerFrame = 15;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(ColourEffectId.WidePartOfZebesExplosionForeground, ColorsPerFrame);
}
