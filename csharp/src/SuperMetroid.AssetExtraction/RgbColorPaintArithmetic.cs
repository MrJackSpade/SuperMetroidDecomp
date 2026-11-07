using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction;

/// <summary>The colour arithmetic the import's paints compute their pre-coloured shades with.</summary>
internal static class RgbColorPaintArithmetic
{
    /// <summary>Packing channels a paint computed.</summary>
    extension(RgbColor)
    {
        /// <summary>
        /// Packs three channels, saturating each into 0-<see cref="RgbColor.MaxChannel"/>. For paint arithmetic whose
        /// authored or native rule clamps; strict packing uses the constructor.
        /// </summary>
        internal static RgbColor Saturating(int red, int green, int blue) =>
            new(Math.Clamp(red, 0, RgbColor.MaxChannel), Math.Clamp(green, 0, RgbColor.MaxChannel), Math.Clamp(blue, 0, RgbColor.MaxChannel));
    }
}
