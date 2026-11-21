using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of Crateria's two escape lightning effects and the colours each writes, for the import.</summary>
public static class CrateriaEscapeLightningPaletteFxColorDefinitions
{

    /// <summary>The yellow-lightning program targets eleven colors from colour byte $A2.</summary>
    internal static readonly CrateriaEscapeLightningPaletteFxColorDefinition CrateriaEscapeYellowLightning = new(ColourEffectId.CrateriaEscapeYellowLightning, 11);
    /// <summary>The Parlor's sky program targets the lightning's five-color dark tail from colour byte $AE.</summary>
    internal static readonly CrateriaEscapeLightningPaletteFxColorDefinition ParlorEscapeSkyFlicker = new(ColourEffectId.ParlorEscapeSkyFlicker, 5);

}
