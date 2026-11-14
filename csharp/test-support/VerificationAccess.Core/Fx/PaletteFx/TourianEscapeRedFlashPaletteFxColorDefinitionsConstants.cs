using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Cartridge values and helpers of <see cref="TourianEscapeRedFlashPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class TourianEscapeRedFlashPaletteFxColorDefinitionsConstants
{
    /// <summary>Both loops contain fourteen timed records.</summary>
    public static int FrameCount => TourianEscapeRedFlashPaletteFxColorDefinitions.All[0].ColorFrames.FrameCount;
}
