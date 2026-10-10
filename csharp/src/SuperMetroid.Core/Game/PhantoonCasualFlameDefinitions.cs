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
    /// <param name="index">Native selector 0..3, indexing the pointer table at $A7:CCFD.</param>
    /// <returns>An immutable view of the selected count, cooldown, and interval words.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside 0..3.</exception>
    public static Schedule Pattern(int index) => (uint)index < 4
        ? new Schedule(index) : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>
    /// Calculated view of a native casual-flame table at $A7:CD05, $A7:CD13,
    /// $A7:CD1D, or $A7:CD2F: flame count, cooldown, then one interval per flame.
    /// Timing words count mouth-scheduler updates; the scheduler reads intervals
    /// from the end toward the header before starting each flame's mouth animation.
    /// </summary>
    /// <param name="pattern">Native selector 0..3. Direct construction assumes a valid selector; <see cref="Pattern"/> validates it.</param>
    public readonly struct Schedule(int pattern) : IReadOnlyList<ushort>
    {
        /// <summary>Maps the native selector to the burst rank used to derive flame count and uniform interval duration.</summary>
        private int BurstRank => pattern == 0 ? 2 : pattern == 1 ? 1 : 3;
        /// <summary>Number of stored words, including the two header words; not the number of flames.</summary>
        public int Count => 2 * BurstRank + 3;
        /// <summary>Reads a native table word: flame count at 0, 180-update cooldown at 1, or an interval thereafter.</summary>
        /// <param name="index">Zero-based word index, less than <see cref="Count"/>.</param>
        /// <returns>The unsigned count or scheduler-update duration at the requested position.</returns>
        /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is negative or at least <see cref="Count"/>.</exception>
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
        /// <summary>Enumerates the header and interval words in forward storage order, not chronological flame order.</summary>
        /// <returns>An enumerator over all <see cref="Count"/> calculated words without exposing mutable storage.</returns>
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index =0;index<Count;index++) yield return this[index];
        }
        /// <summary>Returns a non-generic enumerator over this sequence.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
