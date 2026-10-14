
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="SamusLoadingSuitPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class SamusLoadingSuitPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>Every suit-loading program lasts 265 frames.</summary>
    public const int CycleFrames = 265;

    /// <summary>Provides the palette cycle period for Samus suit loading effects.</summary>
    extension(SamusLoadingSuitPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="SamusLoadingSuitPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => SamusLoadingSuitPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
    }
}
