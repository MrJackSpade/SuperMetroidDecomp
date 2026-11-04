using SuperMetroid.AssetExtraction.Cartridge;
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
        /// <summary>
        /// Streams little-endian colour words through the CGDATA port ($2122) starting at
        /// <paramref name="start"/>; bit 15 of each word is not stored, as on hardware.
        /// </summary>
        public void LoadPortBytes(ReadOnlySpan<byte> littleEndianWords, int start = 0)
        {
            if ((littleEndianWords.Length & 1) != 0)
                throw new ArgumentException("CGRAM data must contain complete two-byte colours.", nameof(littleEndianWords));
            self.CheckSpan(start, littleEndianWords.Length / ColourWordCodec.ByteCount);
            for (int color = 0; color < littleEndianWords.Length / ColourWordCodec.ByteCount; color++)
                self.cgram.SetColor(self.first + start + color, RgbColor.FromWordIgnoringTopBit(
                    LittleEndianBytes.Word(littleEndianWords[color * 2], littleEndianWords[color * 2 + 1])));
        }
    }
}
