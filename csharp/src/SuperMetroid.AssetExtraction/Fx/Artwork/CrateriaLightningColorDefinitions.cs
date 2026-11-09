using SuperMetroid.Core.Fx.PaletteFx;

using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.Artwork;

/// <summary>Calculated surface-lightning colors belonging to the Crateria lightning object.</summary>
internal static class CrateriaLightningColorDefinitions
{
    /// <summary>Thirteen surface-lightning records, each containing eight colors.</summary>
    internal static CrateriaLightningPaletteFxColorDefinition SurfaceProgram =>
        CrateriaLightningPaletteFxColorDefinitions.All[0];

    /// <summary>The unused dark-lightning program: fourteen seven-color records.</summary>
    internal static CrateriaLightningPaletteFxColorDefinition DarkProgram =>
        CrateriaLightningPaletteFxColorDefinitions.All[1];
}
