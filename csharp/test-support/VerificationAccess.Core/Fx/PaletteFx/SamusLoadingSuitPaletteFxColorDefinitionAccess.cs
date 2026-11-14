using SuperMetroid.AssetExtraction.Fx.PaletteFx;
namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Verification access to <see cref="SuperMetroid.AssetExtraction.Fx.PaletteFx.SamusLoadingSuitPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class SamusLoadingSuitPaletteFxColorDefinitionAccess
{
    /// <summary>Exposes <see cref="SuperMetroid.AssetExtraction.Fx.PaletteFx.SamusLoadingSuitPaletteFxColorDefinition"/> state that only verification reads.</summary>
    extension(SamusLoadingSuitPaletteFxColorDefinition self)
    {
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
    }
}
