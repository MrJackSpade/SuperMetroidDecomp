
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Verification access to <see cref="CinematicGlowPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class CinematicGlowPaletteFxColorDefinitionAccess
{
    /// <summary>Calculates the full cinematic glow palette cycle from its staged frame durations.</summary>
    extension(CinematicGlowPaletteFxColorDefinition self)
    {
        /// <summary>The complete loop duration in frames.</summary>
        internal int CycleFrames =>
            CinematicGlowPaletteFxColorDefinitionsConstants.FrameCount * self.FrameDuration;
        /// <summary>The native duration of each timed record: six updates for the lights, five for the gunship.</summary>
        internal int FrameDuration => self.Owner == CinematicGlowPaletteFxProgramOwner.OldMotherBrainBackgroundLights ? 6 : 5;
        /// <summary>The live colours each timed record writes.</summary>
        internal int ColorsPerFrame => self.ColorFrames.ColorsPerFrame;
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
    }
}
