
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Verification access to <see cref="ZebesExplosionLayerFadePaletteFxColorDefinition"/> members production does not use.</summary>
internal static class ZebesExplosionLayerFadePaletteFxColorDefinitionAccess
{
    /// <summary>Calculates the one-shot Zebes explosion layer fade duration from its fixed frames.</summary>
    extension(ZebesExplosionLayerFadePaletteFxColorDefinition self)
    {
        /// <summary>The complete one-shot fade duration in frames.</summary>
        internal int CycleFrames =>
            ZebesExplosionLayerFadePaletteFxColorDefinitions.FrameCount *
            self.FrameDuration;
    }
}
