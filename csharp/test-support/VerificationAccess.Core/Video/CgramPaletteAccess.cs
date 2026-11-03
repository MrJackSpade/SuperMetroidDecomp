using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>Verification access to <see cref="CgramPalette"/> members production does not use.</summary>
internal static class CgramPaletteAccess
{
    /// <summary>Exposes <see cref="CgramPalette"/> state that only verification reads.</summary>
    extension(CgramPalette self)
    {
        /// <summary>Writes <paramref name="colors"/> starting at <paramref name="start"/>.</summary>
        public void CopyFrom(ReadOnlySpan<RgbColor> colors, int start = 0)
        {
            self.CheckSpan(start, colors.Length);
            for (int index = 0; index < colors.Length; index++)
                self.cgram.SetColor(self.first + start + index, colors[index]);
        }
    }
}
