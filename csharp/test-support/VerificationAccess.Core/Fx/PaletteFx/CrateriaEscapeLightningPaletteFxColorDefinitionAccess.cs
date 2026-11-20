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
        /// <summary>The live colours each record writes.</summary>
        internal int ColorsPerFrame => self.ColorFrames.ColorsPerFrame;
    }
}
