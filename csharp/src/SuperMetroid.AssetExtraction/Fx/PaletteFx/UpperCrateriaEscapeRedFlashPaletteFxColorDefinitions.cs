using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of upper Crateria's escape red flash and the colours each writes, for the import.</summary>
public static class UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions
{
    /// <summary>Each record writes seven live BGR555 colors.</summary>
    internal const int ColorsPerFrame = 7;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(ColourEffectId.UpperCrateriaRedFlashing, ColorsPerFrame);
}
