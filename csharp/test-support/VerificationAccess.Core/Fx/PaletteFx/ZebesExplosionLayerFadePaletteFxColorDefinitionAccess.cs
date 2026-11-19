
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Verification access to <see cref="ZebesExplosionLayerFadePaletteFxColorDefinition"/> members production does not use.</summary>
internal static class ZebesExplosionLayerFadePaletteFxColorDefinitionAccess
{
    /// <summary>
    /// What each program the cartridge holds that production does not read (#2570), by each program's owner: the checks compare
    /// each value against the cartridge.
    /// </summary>
    private static readonly Dictionary<ZebesExplosionLayerFadePaletteFxProgramOwner, (ColourEffectId Object, int FrameDuration)> CrossChecks = new()
    {
        [ZebesExplosionLayerFadePaletteFxProgramOwner.Crust] = (ColourEffectId.FadeOutExplodingZebesCrust, 20),
        [ZebesExplosionLayerFadePaletteFxProgramOwner.GreyClouds] = (ColourEffectId.FadeOutExplodingZebesGreyClouds, 14),
    };

    /// <summary>Calculates the one-shot Zebes explosion layer fade duration from its fixed frames.</summary>
    extension(ZebesExplosionLayerFadePaletteFxColorDefinition self)
    {
        /// <summary>The palette-FX object that installs this program.</summary>
        internal ColourEffectId Object => CrossChecks[self.Owner].Object;
        /// <summary>The authored duration of each timed record.</summary>
        internal int FrameDuration => CrossChecks[self.Owner].FrameDuration;
        /// <summary>The complete one-shot fade duration in frames.</summary>
        internal int CycleFrames =>
            ZebesExplosionLayerFadePaletteFxColorDefinitionsConstants.FrameCount *
            self.FrameDuration;
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
    }
}
