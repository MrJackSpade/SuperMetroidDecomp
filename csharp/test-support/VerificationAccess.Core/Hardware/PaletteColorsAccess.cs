using SuperMetroid.Core.Assets;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="IPaletteColors"/> reads production does not make.</summary>
internal static class PaletteColorsAccess
{
    /// <summary>Exposes palette colours as the 8-bit RGBA a captured frame shows.</summary>
    extension(IPaletteColors self)
    {
        /// <summary>The colour at <paramref name="index"/> expanded to 8-bit RGBA.</summary>
        public RgbaPixel GetRgba(int index) => self.Colors[index].ToRgbaPixel();
    }
}
