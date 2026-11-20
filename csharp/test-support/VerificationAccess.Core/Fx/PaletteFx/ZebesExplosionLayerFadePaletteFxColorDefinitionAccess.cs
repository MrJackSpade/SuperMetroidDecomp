
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
        /// <summary>The native duration of each timed record: twenty updates for the crust, fourteen for the grey clouds.</summary>
        internal int FrameDuration => self.Owner == ZebesExplosionLayerFadePaletteFxProgramOwner.Crust ? 20 : 14;
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
    }
}
