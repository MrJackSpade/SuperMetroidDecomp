using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Verification access to <see cref="ScreenPalette"/> by palette position, which production does not use (#1422).</summary>
internal static class ScreenPaletteAccess
{
    /// <summary>Sets, loads and reads a screen's colours by position.</summary>
    extension(ScreenPalette self)
    {
        /// <summary>Writes the colour at <paramref name="index"/>.</summary>
        public void SetColor(int index, RgbColor color) => self.All.Write(index, color);

        /// <summary>Loads consecutive stored colour words from <paramref name="destinationIndex"/> onward.</summary>
        public void LoadBytes(ReadOnlySpan<byte> bytes, int destinationIndex = 0) => self.AsPaletteColors().LoadBytes(bytes, destinationIndex);

        /// <summary>The colour at <paramref name="index"/> expanded to 8-bit RGBA.</summary>
        public RgbaPixel GetRgba(int index) => self.Colors[index].ToRgbaPixel();

        /// <summary>The screen's colours viewed by palette position, as the import pipeline's palette views take them.</summary>
        public IPaletteColors AsPaletteColors() => new ScreenPaletteColors(self);
    }
}
