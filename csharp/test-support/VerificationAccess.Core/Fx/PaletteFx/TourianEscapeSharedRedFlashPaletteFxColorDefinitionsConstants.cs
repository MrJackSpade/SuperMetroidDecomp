
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="TourianEscapeSharedRedFlashPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class TourianEscapeSharedRedFlashPaletteFxColorDefinitionsConstants
{
    /// <summary>The complete shared loop lasts 28 frames.</summary>
    public const int CycleFrames = 28;

    /// <summary>Provides the shared Tourian escape red flash palette cycle period.</summary>
    extension(TourianEscapeSharedRedFlashPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="TourianEscapeSharedRedFlashPaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => TourianEscapeSharedRedFlashPaletteFxColorDefinitionsConstants.CycleFrames;
    }
}
