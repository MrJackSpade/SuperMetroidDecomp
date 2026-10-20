
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="TourianStatueGreyPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class TourianStatueGreyPaletteFxColorDefinitionsConstants
{

    /// <summary>Provides the frame count through deletion for Tourian statue greying checks.</summary>
    extension(TourianStatueGreyPaletteFxColorDefinitions)
    {
        /// <summary>Handler frames from initial setup through the terminal delete.</summary>
        internal static int FramesThroughDeletion => 1 + TourianStatueGreyPaletteFxColorDefinitions.FrameCount * 8 + 1;
    }
}
