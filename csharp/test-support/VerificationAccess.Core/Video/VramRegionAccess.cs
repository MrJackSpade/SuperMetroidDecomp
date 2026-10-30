
namespace SuperMetroid.AssetExtraction.Video;

/// <summary>Verification access to <see cref="VramRegion"/> members production does not use.</summary>
internal static class VramRegionAccess
{
    /// <summary>Exposes <see cref="VramRegion"/> state that only verification reads.</summary>
    extension(VramRegion self)
    {
        /// <summary>
        /// Writes a word stream starting <paramref name="wordOffset"/> words in, advancing <paramref name="wordStep"/> words
        /// after each word: one along a row, a row's width down a column.
        /// </summary>
        public void Write(ReadOnlySpan<ushort> words, int wordOffset = 0, int wordStep = 1)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(wordStep);
            if (words.Length > 0)
                self.CheckWords(wordOffset, (words.Length - 1) * wordStep + 1);
            self.vram.ExecuteWordTransfer(words, (ushort)(self.first + wordOffset), wordStep);
        }
        /// <summary>Rejects a word span that crosses this region's physical allocation.</summary>
        internal void CheckWords(int wordOffset, int wordCount)
        {
            if (wordOffset < 0 || wordCount < 0 || wordOffset + wordCount > self.WordCount)
                throw new ArgumentOutOfRangeException(nameof(wordOffset), wordOffset,
                    $"Words {wordOffset}+{wordCount} exceed the {self.WordCount}-word VRAM region at ${self.first:X4}.");
        }
    }
}
