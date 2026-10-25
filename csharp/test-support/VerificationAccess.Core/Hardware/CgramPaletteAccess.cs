using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="CgramPalette"/> members production does not use.</summary>
internal static class CgramPaletteAccess
{
    /// <summary>Exposes <see cref="CgramPalette"/> state that only verification reads.</summary>
    extension(CgramPalette self)
    {
        /// <summary>The colour at <paramref name="index"/> expanded to 8-bit RGBA.</summary>
        public RgbaPixel Rgba(int index) => self.cgram.GetRgba(self.first + self.Index(index));
    }
}
