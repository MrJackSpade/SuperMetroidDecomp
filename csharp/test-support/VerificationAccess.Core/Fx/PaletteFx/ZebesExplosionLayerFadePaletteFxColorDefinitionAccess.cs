
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Verification access to <see cref="ZebesExplosionLayerFadePaletteFxColorDefinition"/> members production does not use.</summary>
internal static class ZebesExplosionLayerFadePaletteFxColorDefinitionAccess
{
    /// <summary>Calculates the one-shot Zebes explosion layer fade duration from its fixed frames.</summary>
    extension(ZebesExplosionLayerFadePaletteFxColorDefinition self)
    {
        /// <summary>The complete one-shot fade duration in frames.</summary>
        internal int CycleFrames =>
            ZebesExplosionLayerFadePaletteFxColorDefinitionsConstants.FrameCount *
            self.FrameDuration;
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
    }
}
