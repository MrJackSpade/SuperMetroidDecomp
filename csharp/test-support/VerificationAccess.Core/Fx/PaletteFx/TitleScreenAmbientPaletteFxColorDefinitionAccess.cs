namespace SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Frontend.Title;

/// <summary>Verification access to <see cref="TitleScreenAmbientPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class TitleScreenAmbientPaletteFxColorDefinitionAccess
{
    /// <summary>Exposes tube/display light definition pointers and their full title-screen animation cycles.</summary>
    extension(TitleScreenAmbientPaletteFxColorDefinition self)
    {
        /// <summary>Native palette definitions $8D:E1A0 (tube) / $E1A4 (displays).</summary>
        internal ushort DefinitionPointer => PaletteFxObjectAccess.NativeAddress(self.IsTubeLight ? TitleSequenceRomData.ConsolePaletteFx.SlowLights : TitleSequenceRomData.ConsolePaletteFx.FastLights);

        /// <summary>The complete loop duration in frames.</summary>
        internal int CycleFrames => self.FrameCount * self.FrameDuration;
        /// <summary>Native timed-record exposure in palette-FX updates: ten for each tube shade or one for each display state, producing 80-update and two-update loops respectively; rendering does not advance it.</summary>
        public ushort FrameDuration => self.IsTubeLight ? TitleScreenAmbientPaletteFxColorDefinitionsConstants.TubeShadeTicks : (ushort)1;
    }
}
