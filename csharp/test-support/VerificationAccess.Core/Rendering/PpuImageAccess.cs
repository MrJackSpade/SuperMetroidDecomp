using SuperMetroid.Core.Hardware;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Rendering;

/// <summary>Whole-image comparisons of PPU state that only verification performs (#630).</summary>
internal static class PpuImageAccess
{
    /// <summary>Adds full low-table and high-table OAM comparisons for exact image tests.</summary>
    extension(OamBuffer self)
    {
        /// <summary>Whether this OAM holds exactly the same low and high tables as <paramref name="other"/>.</summary>
        internal bool ImageEquals(OamBuffer other) =>
            self.LowTable.SequenceEqual(other.LowTable) && self.HighTable.SequenceEqual(other.HighTable);

        /// <summary>The first object whose record differs from <paramref name="other"/>'s, for a failure message.</summary>
        internal string FirstDifference(OamBuffer other)
        {
            for (int index = 0; index < self.LowTable.Length / 4; index++)
            {
                ReadOnlySpan<byte> mine = self.LowTable.Slice(index * 4, 4), theirs = other.LowTable.Slice(index * 4, 4);
                int high = self.HighTable[index / 4] >> (index % 4 * 2) & 3, otherHigh = other.HighTable[index / 4] >> (index % 4 * 2) & 3;
                if (!mine.SequenceEqual(theirs) || high != otherHigh)
                    return $"object {index}: {Convert.ToHexString(mine)}/{high} against {Convert.ToHexString(theirs)}/{otherHigh}";
            }
            return "no difference";
        }
    }

    /// <summary>Adds a complete 64 KiB VRAM comparison for exact image tests.</summary>
    extension(SnesVram self)
    {
        /// <summary>Whether this VRAM holds exactly the same 64 KiB image as <paramref name="other"/>.</summary>
        internal bool ImageEquals(SnesVram other) => self.Bytes.SequenceEqual(other.Bytes);
    }

    /// <summary>Adds a 256-color CGRAM image comparison for exact image tests.</summary>
    extension(CgramPalettes self)
    {
        /// <summary>Whether this CGRAM holds exactly the same 256-colour image as <paramref name="other"/>.</summary>
        internal bool ImageEquals(CgramPalettes other) => self.Image.SequenceEqual(other.Image);
    }
}
