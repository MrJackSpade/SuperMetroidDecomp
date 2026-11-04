using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>Verification access to <see cref="SnesCgram"/> members production does not use.</summary>
internal static class SnesCgramAccess
{
    /// <summary>Exposes <see cref="SnesCgram"/> state that only verification reads.</summary>
    extension(SnesCgram self)
    {
        /// <summary>Loads consecutive little-endian palette bytes already decoded in host memory.</summary>
        public void LoadBytes(ReadOnlySpan<byte> bytes, int destinationIndex = 0)
        {
            if ((bytes.Length & 1) != 0)
                throw new ArgumentException("CGRAM data must contain complete two-byte colors.", nameof(bytes));
            int colorCount = bytes.Length / 2;
            if (destinationIndex < 0 || destinationIndex + colorCount > SnesCgram.ColorCount)
                throw new ArgumentOutOfRangeException(nameof(destinationIndex), "CGRAM load must remain within 256 colors.");

            for (int color = 0; color < colorCount; color++)
            {
                self.SetColor(destinationIndex + color, RgbColor.FromWordIgnoringTopBit(LittleEndianBytes.Word(bytes[color * 2], bytes[color * 2 + 1])));
            }
        }
    }
}
