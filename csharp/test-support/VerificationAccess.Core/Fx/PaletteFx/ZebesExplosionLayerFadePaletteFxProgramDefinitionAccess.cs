
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Verification access to <see cref="ZebesExplosionLayerFadePaletteFxProgramDefinition"/> members production does not use.</summary>
internal static class ZebesExplosionLayerFadePaletteFxProgramDefinitionAccess
{
    /// <summary>Calculates the one-shot Zebes explosion layer fade duration from its fixed frames.</summary>
    extension(ZebesExplosionLayerFadePaletteFxProgramDefinition self)
    {
        /// <summary>The complete one-shot fade duration in frames.</summary>
        internal int CycleFrames =>
            ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameCount *
            self.FrameDuration;
    }
}
