using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction;

/// <summary>The colour arithmetic the import's paints compute their pre-coloured shades with.</summary>
internal static class RgbColorPaintArithmetic
{
    /// <summary>Making colours from levels a paint computed.</summary>
    extension(RgbColor)
    {
        /// <summary>
        /// The colour of three levels, saturating each into 0-<see cref="FadeLevelColour.FullLevel"/>. For paint
        /// arithmetic whose authored or native rule clamps; strict levels use <see cref="FadeLevelColour"/>'s constructor.
        /// </summary>
        internal static RgbColor Saturating(int red, int green, int blue) =>
            new FadeLevelColour(Math.Clamp(red, 0, FadeLevelColour.FullLevel), Math.Clamp(green, 0, FadeLevelColour.FullLevel), Math.Clamp(blue, 0, FadeLevelColour.FullLevel)).ToColour();
    }
}
