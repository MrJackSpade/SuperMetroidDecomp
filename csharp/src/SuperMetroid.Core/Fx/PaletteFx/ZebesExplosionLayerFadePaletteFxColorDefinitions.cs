namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for the exploding-Zebes crust and cloud fades.</summary>
/// <remarks>
/// Issue #844 / #625: in the pinned NTSC J/U v1.0 ROM, definitions
/// <c>$8D:E1DC</c> (crust) and <c>$8D:E1E0</c> (grey clouds) enter at
/// <c>$8D:D48E</c> and <c>$8D:D5A4</c>. Each runs
/// <c>SetColorIndex(index)</c>, eight 34-byte records of
/// <c>duration, colors[15], Wait</c>, then <c>Delete</c>. Crust uses index
/// <c>$0082</c>, duration 20, and deletes at <c>$D5A2</c> after 160 frames;
/// clouds use index <c>$00A2</c>, duration 14, and delete at <c>$D6B8</c>
/// after 112 frames. All 38 control words match. For either layer, frame
/// <c>f</c> (0..7), column <c>c</c> (0..14), and each BGR555 component
/// <c>q</c>, the color is <c>floor(q(first row, c) * (8 - f) / 8)</c>.
/// This independent channel rule matches all 240 ROM color words exactly.
/// The first rows and their scaled colors remain live presentation data;
/// the presentation compiler supplies them while this catalog supplies only
/// controls to the palette-FX runtime. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
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
            Rows = PaletteFxColorRows.Of(PaletteFxProgram.FadeOutExplodingZebesCrust, ColorsPerFrame),
        },
        new()
        {
            Owner = ZebesExplosionLayerFadePaletteFxProgramOwner.GreyClouds,
            Object = PaletteFxObject.FadeOutExplodingZebesGreyClouds,
            FrameDuration = 14,
            Rows = PaletteFxColorRows.Of(PaletteFxProgram.FadeOutExplodingZebesGreyClouds, ColorsPerFrame),
        },
    ];

    /// <summary>The crust and grey-cloud programs in definition order.</summary>
    public static IReadOnlyList<ZebesExplosionLayerFadePaletteFxColorDefinition> All { get; } = Array.AsReadOnly(Definitions);
}
