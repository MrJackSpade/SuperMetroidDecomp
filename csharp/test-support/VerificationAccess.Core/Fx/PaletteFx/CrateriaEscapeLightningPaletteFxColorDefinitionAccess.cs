using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Verification access to <see cref="CrateriaEscapeLightningPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class CrateriaEscapeLightningPaletteFxColorDefinitionAccess
{
    /// <summary>Exposes <see cref="CrateriaEscapeLightningPaletteFxColorDefinition"/> state that only verification reads.</summary>
    extension(CrateriaEscapeLightningPaletteFxColorDefinition self)
    {
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
    }
}
