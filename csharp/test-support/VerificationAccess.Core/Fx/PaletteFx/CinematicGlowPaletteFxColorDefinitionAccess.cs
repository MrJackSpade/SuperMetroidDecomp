
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Verification access to <see cref="CinematicGlowPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class CinematicGlowPaletteFxColorDefinitionAccess
{
    /// <summary>Calculates the full cinematic glow palette cycle from its staged frame durations.</summary>
    extension(CinematicGlowPaletteFxColorDefinition self)
    {
        /// <summary>The complete loop duration in frames.</summary>
        internal int CycleFrames =>
            CinematicGlowPaletteFxColorDefinitions.FrameCount * self.FrameDuration;
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.Rows.Color(frame, color);
    }
}
