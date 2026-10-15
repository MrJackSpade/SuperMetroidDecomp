
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="TourianStatueGreyPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class TourianStatueGreyPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>Handler frames from initial setup through the terminal delete.</summary>
    public const int FramesThroughDeletion = 1 + TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FrameCount * 8 + 1;

    /// <summary>Provides the frame count through deletion for Tourian statue greying checks.</summary>
    extension(TourianStatueGreyPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="TourianStatueGreyPaletteFxProgramMechanicsDefinitionsConstants.FramesThroughDeletion"/>
        internal static int FramesThroughDeletion => TourianStatueGreyPaletteFxProgramMechanicsDefinitionsConstants.FramesThroughDeletion;
    }
}
