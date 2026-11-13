using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable mechanics for the exploding-Zebes crust and cloud fades.</summary>
/// <remarks>
/// Issue #844 / #625: the crust and grey-cloud
/// definitions share one layout.
/// Each runs
/// <c>SetColorIndex(index)</c>, eight 34-byte records of
/// <c>duration, colors[15], Wait</c>, then <c>Delete</c>. Crust uses index
/// 130, duration 20, and deletes after 160 frames;
/// clouds use index 162, duration 14, and delete
/// after 112 frames. All 38 control words match. For either layer, frame
/// <c>f</c> (0..7), column <c>c</c> (0..14), and each BGR555 component
/// <c>q</c>, the color is <c>floor(q(first row, c) * (8 - f) / 8)</c>.
/// This independent channel rule matches all 240 original color words exactly.
/// The first rows and their scaled colors remain live presentation data;
/// the presentation compiler supplies them while this catalog supplies only
/// controls to the palette-FX runtime.
/// </remarks>
public static class ZebesExplosionLayerFadePaletteFxColorDefinitions
{

    /// <summary>Each record writes fifteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 15;

    /// <summary>Crust and grey-cloud one-shot fades with distinct destinations and authored holds.</summary>
    private static readonly ZebesExplosionLayerFadePaletteFxColorDefinition[] Definitions =
    [
        new()
        {
            Owner = ZebesExplosionLayerFadePaletteFxProgramOwner.Crust,
            Object = PaletteFxObject.FadeOutExplodingZebesCrust,
            FrameDuration = 20,
            ColorFrames = PaletteFxColorFrames.Of(PaletteFxSegment.FadeOutExplodingZebesCrust, ColorsPerFrame),
        },
        new()
        {
            Owner = ZebesExplosionLayerFadePaletteFxProgramOwner.GreyClouds,
            Object = PaletteFxObject.FadeOutExplodingZebesGreyClouds,
            FrameDuration = 14,
            ColorFrames = PaletteFxColorFrames.Of(PaletteFxSegment.FadeOutExplodingZebesGreyClouds, ColorsPerFrame),
        },
    ];

    /// <summary>The crust and grey-cloud programs in definition order.</summary>
    public static IReadOnlyList<ZebesExplosionLayerFadePaletteFxColorDefinition> All { get; } = Array.AsReadOnly(Definitions);
}
