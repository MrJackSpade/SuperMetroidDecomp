using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The three suits' loading records and the colours each writes, for the import.</summary>
public static partial class SamusLoadingSuitPaletteFxColorDefinitions
{

    /// <summary>Each record writes sixteen live colours.</summary>
    internal const int ColorsPerFrame = 16;

    /// <summary>The Power Suit's loading colours.</summary>
    internal static SamusLoadingSuitPaletteFxColorDefinition PowerSuit { get; } = new(ColourEffectId.SamusLoadingPowerSuit, ColorsPerFrame);

    /// <summary>The Varia Suit's loading colours.</summary>
    internal static SamusLoadingSuitPaletteFxColorDefinition VariaSuit { get; } = new(ColourEffectId.SamusLoadingVariaSuit, ColorsPerFrame);

    /// <summary>The Gravity Suit's loading colours.</summary>
    internal static SamusLoadingSuitPaletteFxColorDefinition GravitySuit { get; } = new(ColourEffectId.SamusLoadingGravitySuit, ColorsPerFrame);
}
