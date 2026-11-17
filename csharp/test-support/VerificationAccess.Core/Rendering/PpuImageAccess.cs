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

        /// <summary>
        /// Whether this OAM draws the picture <paramref name="other"/> draws: the same objects, in an order that differs only
        /// between objects that do not overlap, whose order no pixel shows (#2273). One installed picture stands for every
        /// record the cartridge stores it in, some listing its objects in another order.
        /// </summary>
        internal bool DrawsSamePicture(OamBuffer other)
        {
            int count = self.NextByteOffset / OamBuffer.LowRecordByteCount;
            if (other.NextByteOffset != self.NextByteOffset)
                return false;
            var unmatched = new List<int>(Enumerable.Range(0, count));
            var placeInOther = new int[count];
            for (int index = 0; index < count; index++)
            {
                OamEntry entry = self.GetEntry(index);
                int match = unmatched.FindIndex(candidate => other.GetEntry(candidate) == entry);
                if (match < 0)
                    return false;
                placeInOther[index] = unmatched[match];
                unmatched.RemoveAt(match);
            }
            for (int first = 0; first < count; first++)
            {
                for (int second = first + 1; second < count; second++)
                {
                    if (placeInOther[first] > placeInOther[second] && Overlap(self.GetEntry(first), self.GetEntry(second)))
                        return false;
                }
            }
            return true;
        }

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

    /// <summary>The span of a small object, in pixels, under the game's eight-and-sixteen object sizes.</summary>
    private const int SmallObjectSpan = 8;
    /// <summary>The span of a large object, in pixels, under the game's eight-and-sixteen object sizes.</summary>
    private const int LargeObjectSpan = 16;
    /// <summary>The nine-bit OAM X range, past which an object's columns continue from zero.</summary>
    private const int ObjectXRange = 512;
    /// <summary>The eight-bit OAM Y range, past which an object's rows continue from zero.</summary>
    private const int ObjectYRange = 256;

    /// <summary>Whether two objects cover a common pixel, their columns and rows wrapping as OAM's do.</summary>
    private static bool Overlap(OamEntry first, OamEntry second)
    {
        int firstSpan = first.IsLarge ? LargeObjectSpan : SmallObjectSpan, secondSpan = second.IsLarge ? LargeObjectSpan : SmallObjectSpan;
        return Covers(first.Position.X, firstSpan, second.Position.X, secondSpan, ObjectXRange)
            && Covers(first.Position.Y, firstSpan, second.Position.Y, secondSpan, ObjectYRange);

        static bool Covers(int a, int aSpan, int b, int bSpan, int range) =>
            (((b - a) % range) + range) % range < aSpan || (((a - b) % range) + range) % range < bSpan;
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
