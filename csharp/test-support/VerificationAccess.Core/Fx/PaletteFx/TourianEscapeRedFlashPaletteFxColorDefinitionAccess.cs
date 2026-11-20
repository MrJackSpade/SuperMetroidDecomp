
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Verification access to <see cref="TourianEscapeRedFlashPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class TourianEscapeRedFlashPaletteFxColorDefinitionAccess
{
    /// <summary>Calculates the Tourian escape red-flash cycle from its frame count and per-frame duration.</summary>
    extension(TourianEscapeRedFlashPaletteFxColorDefinition self)
    {
        /// <summary>Frames from the first record through the next first record.</summary>
        internal int CycleFrames =>
            TourianEscapeRedFlashPaletteFxColorDefinitionsConstants.FrameCount * self.Duration;
        /// <summary>The native display duration of every record: two updates for the shutters, four for the background.</summary>
        internal int Duration => self.Owner == TourianEscapeRedFlashPaletteOwner.Shutter ? 2 : 4;
        /// <summary>The live colours each record writes.</summary>
        internal int ColorsPerFrame => self.ColorFrames.ColorsPerFrame;
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
    }
}
