
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Verification access to <see cref="TourianEscapeRedFlashPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class TourianEscapeRedFlashPaletteFxColorDefinitionAccess
{
    /// <summary>
    /// What each program the cartridge holds that production does not read (#2570), by each program's owner: the checks compare
    /// each value against the cartridge.
    /// </summary>
    private static readonly Dictionary<TourianEscapeRedFlashPaletteOwner, (ColourEffectId Object, int Duration, int ColorsPerFrame)> CrossChecks = new()
    {
        [TourianEscapeRedFlashPaletteOwner.Shutter] = (ColourEffectId.TourianShutterRedFlashing, 2, 6),
        [TourianEscapeRedFlashPaletteOwner.Background] = (ColourEffectId.TourianBackgroundRedFlashing, 4, 4),
    };

    /// <summary>Calculates the Tourian escape red-flash cycle from its frame count and per-frame duration.</summary>
    extension(TourianEscapeRedFlashPaletteFxColorDefinition self)
    {
        /// <summary>The palette-FX object that installs this program.</summary>
        internal ColourEffectId Object => CrossChecks[self.Owner].Object;
        /// <summary>The uniform display duration for every record.</summary>
        internal int Duration => CrossChecks[self.Owner].Duration;
        /// <summary>The number of live BGR555 colors in every record.</summary>
        internal int ColorsPerFrame => CrossChecks[self.Owner].ColorsPerFrame;
        /// <summary>Frames from the first record through the next first record.</summary>
        internal int CycleFrames =>
            TourianEscapeRedFlashPaletteFxColorDefinitionsConstants.FrameCount * self.Duration;
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
    }
}
