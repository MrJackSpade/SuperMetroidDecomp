using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>Verification access to <see cref="SnesVram"/> members production does not use.</summary>
internal static class SnesVramAccess
{
    /// <summary>Wraps a word address into video memory's 32K words.</summary>
    private const int WordAddressMask = SnesVram.WordCount - 1;

    /// <summary>Exposes <see cref="SnesVram"/> state that only verification reads.</summary>
    extension(SnesVram self)
    {
        /// <summary>
        /// Executes a word-oriented DMA staging slice with the VMAIN increment already decoded.
        /// </summary>
        /// <param name="words">Complete source-word slice, in DMA order.</param>
        /// <param name="destinationWord">Initial 15-bit VMADD word address.</param>
        /// <param name="wordIncrement">One for a row (VMAIN=$80), 32 for a column (VMAIN=$81).</param>
        public void ExecuteWordTransfer(ReadOnlySpan<ushort> words, int destinationWord, int wordIncrement)
        {
            if (wordIncrement is not (1 or 32))
                throw new ArgumentOutOfRangeException(nameof(wordIncrement), "Known tilemap DMA increments are one or 32 words.");

            int destination = destinationWord & WordAddressMask;
            foreach (int word in words)
            {
                self.LoadBytes(destination * sizeof(ushort), [LittleEndianBytes.LowByte(word), LittleEndianBytes.HighByte(word)]);
                destination = (destination + wordIncrement) & WordAddressMask;
            }
        }
        /// <summary>Loads bytes through Mode 7's high-byte-only $2119 DMA port.</summary>
        public void LoadFloorCharacterBytes(ReadOnlySpan<byte> bytes, int destinationWord = 0) =>
            LoadPortBytes(self, bytes, destinationWord, byteInWord: 1);
        /// <summary>Loads bytes through Mode 7's low-byte-only $2118 DMA port.</summary>
        public void LoadFloorMapBytes(ReadOnlySpan<byte> bytes, int destinationWord = 0) =>
            LoadPortBytes(self, bytes, destinationWord, byteInWord: 0);
    }

    /// <summary>
    /// Loads <paramref name="bytes"/> into byte <paramref name="byteInWord"/> (zero low, one high) of each word of
    /// <paramref name="vram"/> from <paramref name="destinationWord"/>, as Mode 7's single-byte DMA ports write them.
    /// </summary>
    private static void LoadPortBytes(SnesVram vram, ReadOnlySpan<byte> bytes, int destinationWord, int byteInWord)
    {
        if (bytes.Length > SnesVram.WordCount)
            throw new ArgumentOutOfRangeException(nameof(bytes));
        int destination = destinationWord & WordAddressMask;
        for (int index = 0; index < bytes.Length; index++)
        {
            vram.LoadBytes(destination * sizeof(ushort) + byteInWord, bytes.Slice(index, 1));
            destination = (destination + 1) & WordAddressMask;
        }
    }
}
