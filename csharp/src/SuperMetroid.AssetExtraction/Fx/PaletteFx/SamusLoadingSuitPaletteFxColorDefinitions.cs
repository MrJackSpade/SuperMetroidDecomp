using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The three suits' loading records and the colours each writes, for the import.</summary>
public static class SamusLoadingSuitPaletteFxColorDefinitions
{

    /// <summary>Each record writes sixteen live colours.</summary>
    private const int ColorsPerFrame = 16;
    /// <summary>The power, Varia, and gravity programs in definition order, each the suit's loading effect's colours.</summary>
    public static IReadOnlyList<SamusLoadingSuitPaletteFxColorDefinition> All { get; } =
    [
        Suit(SamusLoadingSuitPaletteFxProgramOwner.PowerSuit, ColourEffectId.SamusLoadingPowerSuit),
        Suit(SamusLoadingSuitPaletteFxProgramOwner.VariaSuit, ColourEffectId.SamusLoadingVariaSuit),
        Suit(SamusLoadingSuitPaletteFxProgramOwner.GravitySuit, ColourEffectId.SamusLoadingGravitySuit),
    ];

    /// <summary>The program <paramref name="owner"/> loads: the colours <paramref name="effect"/> writes.</summary>
    private static SamusLoadingSuitPaletteFxColorDefinition Suit(SamusLoadingSuitPaletteFxProgramOwner owner, ColourEffectId effect) =>
        new() { Owner = owner, ColorFrames = PaletteFxColorFrames.Of(effect, ColorsPerFrame) };

}
