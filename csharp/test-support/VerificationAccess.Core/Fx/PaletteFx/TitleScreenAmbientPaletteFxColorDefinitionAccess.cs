using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

namespace SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Frontend.Title;

/// <summary>Verification access to <see cref="TitleScreenAmbientPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class TitleScreenAmbientPaletteFxColorDefinitionAccess
{
    /// <summary>
    /// What each program the cartridge holds that production does not read (#2570), by each program's effect: the checks compare
    /// each value against the cartridge.
    /// </summary>
    private static readonly Dictionary<ColourEffectId, ColourEffectColours> CrossChecks = new()
    {
        [ColourEffectId.TitleScreenBabyMetroidTubeLight] = ColourEffectColours.BabyMetroidTubeLight,
        [ColourEffectId.TitleScreenFlickeringDisplays] = ColourEffectColours.TitleScreenDisplays,
    };

    /// <summary>Exposes tube/display light definition pointers and their full title-screen animation cycles.</summary>
    extension(TitleScreenAmbientPaletteFxColorDefinition self)
    {
        /// <summary>The colours the program animates.</summary>
        internal ColourEffectColours Colours => CrossChecks[self.Effect];
        /// <summary>Native palette definitions $8D:E1A0 (tube) / $E1A4 (displays).</summary>
        internal ushort DefinitionPointer => PaletteFxObjectAccess.NativeAddress(self.Effect == ColourEffectId.TitleScreenBabyMetroidTubeLight ? TitleSequenceDefinitions.ConsoleColourEffects.SlowLights : TitleSequenceDefinitions.ConsoleColourEffects.FastLights);

        /// <summary>The complete loop duration in frames.</summary>
        internal int CycleFrames => self.FrameCount * self.FrameDuration;
        /// <summary>Native timed-record exposure in palette-FX updates: ten for each tube shade or one for each display state, producing 80-update and two-update loops respectively; rendering does not advance it.</summary>
        public ushort FrameDuration => self.Effect == ColourEffectId.TitleScreenBabyMetroidTubeLight ? TitleScreenAmbientPaletteFxColorDefinitionsConstants.TubeShadeTicks : (ushort)1;
        /// <summary>Bytes from one duration through its terminal wait command.</summary>
        public int FrameByteCount => sizeof(ushort) + self.ColorsPerFrame * sizeof(ushort) +
            sizeof(ushort);
    }
}
