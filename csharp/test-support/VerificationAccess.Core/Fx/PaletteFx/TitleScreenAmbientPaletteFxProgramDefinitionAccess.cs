namespace SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Frontend.Title;

/// <summary>Verification access to <see cref="TitleScreenAmbientPaletteFxProgramDefinition"/> members production does not use.</summary>
internal static class TitleScreenAmbientPaletteFxProgramDefinitionAccess
{
    /// <summary>Exposes tube/display light definition pointers and their full title-screen animation cycles.</summary>
    extension(TitleScreenAmbientPaletteFxProgramDefinition self)
    {
        /// <summary>Native palette definitions $8D:E1A0 (tube) / $E1A4 (displays).</summary>
        internal ushort DefinitionPointer => self.IsTubeLight ? TitleSequenceRomData.ConsolePaletteFx.SlowLights : TitleSequenceRomData.ConsolePaletteFx.FastLights;

        /// <summary>The complete loop duration in frames.</summary>
        internal int CycleFrames => self.FrameCount * self.FrameDuration;
    }
}
