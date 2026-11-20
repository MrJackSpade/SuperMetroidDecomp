using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Verification access to <see cref="TorizoBellyPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class TorizoBellyPaletteFxColorDefinitionAccess
{
    /// <summary>Exposes <see cref="TorizoBellyPaletteFxColorDefinition"/> state that only verification reads.</summary>
    extension(TorizoBellyPaletteFxColorDefinition self)
    {
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
    }
}
