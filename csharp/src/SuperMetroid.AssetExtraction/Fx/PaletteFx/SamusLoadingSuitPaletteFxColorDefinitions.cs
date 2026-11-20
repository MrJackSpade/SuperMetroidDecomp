using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The three suits' loading records and the colours each writes, for the import.</summary>
public static class SamusLoadingSuitPaletteFxColorDefinitions
{

    /// <summary>Each record writes sixteen live colours.</summary>
    private const int ColorsPerFrame = 16;
    /// <summary>The Power-suit definition's program.</summary>
    private static readonly SamusLoadingSuitPaletteFxColorDefinition Power =
        new()
        {
            Owner = SamusLoadingSuitPaletteFxProgramOwner.PowerSuit,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectId.SamusLoadingPowerSuit, ColorsPerFrame),
        };
    /// <summary>The Varia-suit definition's program.</summary>
    private static readonly SamusLoadingSuitPaletteFxColorDefinition Varia =
        new()
        {
            Owner = SamusLoadingSuitPaletteFxProgramOwner.VariaSuit,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectId.SamusLoadingVariaSuit, ColorsPerFrame),
        };
    /// <summary>The Gravity-suit definition's program.</summary>
    private static readonly SamusLoadingSuitPaletteFxColorDefinition Gravity =
        new()
        {
            Owner = SamusLoadingSuitPaletteFxProgramOwner.GravitySuit,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectId.SamusLoadingGravitySuit, ColorsPerFrame),
        };

    /// <summary>The power, Varia, and gravity programs in definition order.</summary>

    public static IReadOnlyList<SamusLoadingSuitPaletteFxColorDefinition> All { get; } = [Power, Varia, Gravity];

}
