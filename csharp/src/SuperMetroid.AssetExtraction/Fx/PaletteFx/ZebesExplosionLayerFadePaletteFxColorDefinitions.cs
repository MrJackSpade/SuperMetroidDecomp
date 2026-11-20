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
    private const int ColorsPerFrame = 15;

    /// <summary>Crust and grey-cloud one-shot fades with distinct destinations and authored holds.</summary>
    private static readonly ZebesExplosionLayerFadePaletteFxColorDefinition[] Definitions =
    [
        new()
        {
            Owner = ZebesExplosionLayerFadePaletteFxProgramOwner.Crust,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectId.FadeOutExplodingZebesCrust, ColorsPerFrame),
        },
        new()
        {
            Owner = ZebesExplosionLayerFadePaletteFxProgramOwner.GreyClouds,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectId.FadeOutExplodingZebesGreyClouds, ColorsPerFrame),
        },
    ];

    /// <summary>The crust and grey-cloud programs in definition order.</summary>
    public static IReadOnlyList<ZebesExplosionLayerFadePaletteFxColorDefinition> All { get; } = Array.AsReadOnly(Definitions);
}
