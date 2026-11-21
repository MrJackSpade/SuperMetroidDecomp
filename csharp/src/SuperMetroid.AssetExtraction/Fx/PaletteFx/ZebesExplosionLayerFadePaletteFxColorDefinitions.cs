using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the exploding-Zebes crust and cloud fades and the colours each writes, for the import.</summary>
/// <remarks>
/// Each layer's records dim its first row's colours towards black in equal steps on each channel; this describes the
/// authored colours, not arithmetic the game performs.
/// </remarks>
public static class ZebesExplosionLayerFadePaletteFxColorDefinitions
{

    /// <summary>Each record writes fifteen live BGR555 colors.</summary>
    internal const int ColorsPerFrame = 15;

    /// <summary>The crust's one-shot fade.</summary>
    internal static ZebesExplosionLayerFadePaletteFxColorDefinition FadeOutExplodingZebesCrust { get; } = new(ColourEffectId.FadeOutExplodingZebesCrust, ColorsPerFrame);

    /// <summary>The grey clouds' one-shot fade.</summary>
    internal static ZebesExplosionLayerFadePaletteFxColorDefinition FadeOutExplodingZebesGreyClouds { get; } = new(ColourEffectId.FadeOutExplodingZebesGreyClouds, ColorsPerFrame);
}
