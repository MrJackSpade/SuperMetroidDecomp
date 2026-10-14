
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Verification access to <see cref="TourianEscapeRedFlashPaletteFxProgramDefinition"/> members production does not use.</summary>
internal static class TourianEscapeRedFlashPaletteFxProgramDefinitionAccess
{
    /// <summary>Calculates the Tourian escape red-flash cycle from its frame count and per-frame duration.</summary>
    extension(TourianEscapeRedFlashPaletteFxProgramDefinition self)
    {
        /// <summary>Frames from the first record through the next first record.</summary>
        internal int CycleFrames =>
            TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount * self.Duration;
    }
}
