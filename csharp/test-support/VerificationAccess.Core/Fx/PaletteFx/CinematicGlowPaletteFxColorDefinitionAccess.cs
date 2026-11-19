
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Verification access to <see cref="CinematicGlowPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class CinematicGlowPaletteFxColorDefinitionAccess
{
    /// <summary>
    /// What each program the cartridge holds that production does not read (#2570), by each program's owner: the checks compare
    /// each value against the cartridge.
    /// </summary>
    private static readonly Dictionary<CinematicGlowPaletteFxProgramOwner, (ColourEffectId Object, int ColorsPerFrame, int FrameDuration)> CrossChecks = new()
    {
        [CinematicGlowPaletteFxProgramOwner.OldMotherBrainBackgroundLights] = (ColourEffectId.OldMotherBrainFightBackgroundLights, 3, 6),
        [CinematicGlowPaletteFxProgramOwner.GunshipGlow] = (ColourEffectId.GunshipGlow, 1, 5),
    };

    /// <summary>Calculates the full cinematic glow palette cycle from its staged frame durations.</summary>
    extension(CinematicGlowPaletteFxColorDefinition self)
    {
        /// <summary>The palette-FX object that installs this program.</summary>
        internal ColourEffectId Object => CrossChecks[self.Owner].Object;
        /// <summary>The number of live BGR555 colors written by each timed record.</summary>
        internal int ColorsPerFrame => CrossChecks[self.Owner].ColorsPerFrame;
        /// <summary>The authored duration of each timed record.</summary>
        internal int FrameDuration => CrossChecks[self.Owner].FrameDuration;
        /// <summary>The complete loop duration in frames.</summary>
        internal int CycleFrames =>
            CinematicGlowPaletteFxColorDefinitionsConstants.FrameCount * self.FrameDuration;
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
    }
}
