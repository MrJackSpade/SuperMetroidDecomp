using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the zoomed-out exploding-Zebes fade-out and the colours each writes, for the import.</summary>
/// <remarks>
/// Each record darkens every colour but the first by the same step on red, green and blue, clamped at black; this
/// describes the authored colours, not arithmetic the game performs.
/// </remarks>
public static class ExplodingZebesFadePaletteFxColorDefinitions
{

    /// <summary>Each record writes eight live BGR555 colors.</summary>
    private const int ColorsPerFrame = 8;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(ColourEffectId.FadeOutZoomedOutExplodingZebes, ColorsPerFrame);
}
