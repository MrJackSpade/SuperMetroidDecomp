using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Cartridge values and helpers of <see cref="CinematicGlowPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class CinematicGlowPaletteFxColorDefinitionsConstants
{
    /// <summary>Both glow loops contain fourteen timed records.</summary>
    public static int FrameCount => CinematicGlowPaletteFxColorDefinitions.All[0].ColorFrames.FrameCount;
}
