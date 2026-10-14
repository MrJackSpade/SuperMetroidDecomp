using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="CinematicGlowPaletteFxProgramDefinition"/> members production does not use.</summary>
internal static class CinematicGlowPaletteFxProgramDefinitionAccess
{
    /// <summary>Calculates the full cinematic glow palette cycle from its staged frame durations.</summary>
    extension(CinematicGlowPaletteFxProgramDefinition self)
    {
        /// <summary>The complete loop duration in frames.</summary>
        internal int CycleFrames =>
            CinematicGlowPaletteFxProgramMechanicsDefinitions.FrameCount * self.FrameDuration;
    }
}
