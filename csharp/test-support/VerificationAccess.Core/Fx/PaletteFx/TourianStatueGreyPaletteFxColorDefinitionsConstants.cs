using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Cartridge values of <see cref="TourianStatueGreyPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class TourianStatueGreyPaletteFxColorDefinitionsConstants
{

    /// <summary>Provides the frame count through deletion for Tourian statue greying checks.</summary>
    extension(TourianStatueGreyPaletteFxColorDefinitions)
    {
        /// <summary>Handler frames from initial setup through the terminal delete.</summary>
        internal static int FramesThroughDeletion => 1 + TourianStatueGreyPaletteFxColorDefinitions.ColorFrames.FrameCount * 8 + 1;
    }
    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public static PaletteFxColorKey Color(int frame, int color) => TourianStatueGreyPaletteFxColorDefinitions.ColorFrames.Color(frame, color);
}
