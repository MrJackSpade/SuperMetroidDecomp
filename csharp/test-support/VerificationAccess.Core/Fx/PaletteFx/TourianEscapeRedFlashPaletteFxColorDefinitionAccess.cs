
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Verification access to <see cref="TourianEscapeRedFlashPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class TourianEscapeRedFlashPaletteFxColorDefinitionAccess
{
    /// <summary>Calculates the Tourian escape red-flash cycle from its frame count and per-frame duration.</summary>
    extension(TourianEscapeRedFlashPaletteFxColorDefinition self)
    {
        /// <summary>Frames from the first record through the next first record.</summary>
        internal int CycleFrames =>
            TourianEscapeRedFlashPaletteFxColorDefinitionsConstants.FrameCount * self.Duration;
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.Rows.Color(frame, color);
    }
}
