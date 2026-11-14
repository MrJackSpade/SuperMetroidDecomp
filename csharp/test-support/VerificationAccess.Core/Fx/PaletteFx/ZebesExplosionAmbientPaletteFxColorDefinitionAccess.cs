using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Verification access to <see cref="ZebesExplosionAmbientPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class ZebesExplosionAmbientPaletteFxColorDefinitionAccess
{
    /// <summary>Exposes <see cref="ZebesExplosionAmbientPaletteFxColorDefinition"/> state that only verification reads.</summary>
    extension(ZebesExplosionAmbientPaletteFxColorDefinition self)
    {
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
        /// <summary>The number of timed records in the loop.</summary>
        public int FrameCount => self.ColorFrames.FrameCount;
    }
}
