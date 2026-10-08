namespace SuperMetroid.Core.Game;

/// <summary>Phantoon mouth schedules retain the native header and reverse-read timer layout.</summary>
public static class PhantoonCasualFlameDefinitions
{
    /// <summary>$A7:CD2F's seven irregular reverse-read intervals; independent timing disposition remains required.</summary>
    private static ReadOnlySpan<ushort> IrregularIntervals => [16,64,32,64,32,16,32];

    /// <summary>
    /// $A7:CCFD selects five-, three-, seven-flame uniform bursts, then an irregular
    /// seven-flame burst. All have180-frame cooldowns. Uniform holds are16 ticks per
    /// burst rank, with rank1..3 containing2*rank+1 flames.
    /// </summary>
    public static Schedule Pattern(int index) => (uint)index < 4
        ? new Schedule(index) : throw new ArgumentOutOfRangeException(nameof(index));

    public readonly struct Schedule(int pattern) : IReadOnlyList<ushort>
    {
        private int BurstRank => pattern == 0 ? 2 : pattern == 1 ? 1 : 3;
        public int Count => 2 * BurstRank + 3;
        public ushort this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                if (index == 0) return (ushort)(2 * BurstRank + 1);
                if (index == 1) return 180;
                return pattern == 3 ? IrregularIntervals[index-2] : (ushort)(16 * BurstRank);
            }
        }
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index =0;index<Count;index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}