using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="VramRegion"/> members production does not use.</summary>
internal static class VramRegionAccess
{
    /// <summary>Exposes <see cref="VramRegion"/> state that only verification reads.</summary>
    extension(VramRegion self)
    {
        /// <summary>The word at <paramref name="wordOffset"/>.</summary>
        public ushort ReadWord(int wordOffset)
        {
            self.CheckWords(wordOffset, 1);
            return self.Vram.ReadWord(self.First + wordOffset);
        }
        /// <summary>A sub-region of <paramref name="wordCount"/> words starting <paramref name="wordOffset"/> words in.</summary>
        public VramRegion Slice(int wordOffset, int wordCount)
        {
            self.CheckWords(wordOffset, wordCount);
            return new(self.Vram, new PpuLayoutCell("VRAM slice", PpuLayoutRegion.Vram, self.First + wordOffset, wordCount, "VRAM words"));
        }
    }
}
