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
            return self.vram.ReadWord(self.first + wordOffset);
        }
    }
}
