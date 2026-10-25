namespace SuperMetroid.Core.Frontend.Title;

/// <summary>The title Baby Metroid's four-phase animation page cycle: pages 0, 1, 2, 1.</summary>
public readonly struct TitleBabyPageSequence : IReadOnlyList<byte>
{
    /// <summary>Four animation phases in the page cycle 0, 1, 2, 1.</summary>
    public int Count => 4;
    /// <summary>Animation phase count, equivalent to <see cref="Count"/>, for frame wrapping.</summary>
    public int Length => Count;
    /// <summary>Returns the page shown in a zero-based animation phase of the cycle 0, 1, 2, 1.</summary>
    /// <param name="frame">Animation phase 0-3, rather than an unrestricted elapsed-frame count.</param>
    /// <returns>Page 0-2; multiplying it by a page's bytes gives the page's start.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The phase is outside 0-3.</exception>
    public byte this[int frame] => (uint)frame < Length
        ? (byte)Math.Min(frame, Length - frame)
        : throw new ArgumentOutOfRangeException(nameof(frame));
    /// <summary>Enumerates the four pages in animation order without retaining a page array.</summary>
    /// <returns>An enumerator yielding 0, 1, 2, 1.</returns>
    public IEnumerator<byte> GetEnumerator()
    {
        for (int frame = 0; frame < Count; frame++) yield return this[frame];
    }
    /// <summary>Exposes the same four-page sequence through the non-generic collection contract.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
