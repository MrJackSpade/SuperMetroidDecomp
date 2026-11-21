using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the old-Tourian escape shaft's red flash and the colours each writes, for the import.</summary>
public static partial class OldTourianEscapeRedFlashPaletteFxColorDefinitions
{

    /// <summary>Each record writes eight live BGR555 colors.</summary>
    internal const int ColorsPerFrame = 8;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(ColourEffectId.OldTourianRedFlashing, ColorsPerFrame);
}
