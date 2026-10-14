
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The complete shared loop lasts 28 frames.</summary>
    public const int CycleFrames = 28;

    /// <summary>Provides the shared Tourian escape red flash palette cycle period.</summary>
    extension(TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
    }
}
