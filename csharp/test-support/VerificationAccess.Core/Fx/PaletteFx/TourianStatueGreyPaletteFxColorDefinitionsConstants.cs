
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="TourianStatueGreyPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class TourianStatueGreyPaletteFxColorDefinitionsConstants
{
    /// <summary>Handler frames from initial setup through the terminal delete.</summary>
    public const int FramesThroughDeletion = 1 + TourianStatueGreyPaletteFxColorDefinitions.FrameCount * 8 + 1;

    /// <summary>Provides the frame count through deletion for Tourian statue greying checks.</summary>
    extension(TourianStatueGreyPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="TourianStatueGreyPaletteFxColorDefinitionsConstants.FramesThroughDeletion"/>
        internal static int FramesThroughDeletion => TourianStatueGreyPaletteFxColorDefinitionsConstants.FramesThroughDeletion;
    }
}
