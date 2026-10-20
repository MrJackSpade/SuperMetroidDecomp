namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Ordered rectangle view that attaches calculated callbacks without storing them per record.</summary>
internal readonly record struct KraidArmHitboxSequence(int Start, int Length) : IEnumerable<KraidArmCollisionHitbox>
{
    /// <summary>LINQ traversal of the selected rectangles; boxes one enumerator.</summary>
    IEnumerator<KraidArmCollisionHitbox> IEnumerable<KraidArmCollisionHitbox>.GetEnumerator()
    {
        for (int index = 0; index < Length; index++)
            yield return this[index];
    }

    /// <summary>Non-generic traversal of the selected rectangles.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => ((IEnumerable<KraidArmCollisionHitbox>)this).GetEnumerator();

    /// <summary>Maps an index in a selected hitbox list to its ordered physical rectangle.</summary>
    internal KraidArmCollisionHitbox this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Length) throw new ArgumentOutOfRangeException(nameof(index));
            return KraidArmCollisionDefinitions.Rectangle(Start + index);
        }
    }
    /// <summary>Enumerates a selected list without allocating a rectangle array.</summary>
    public Enumerator GetEnumerator() => new(this);
    /// <summary>Advances through the selected rectangle slice in native collision order.</summary>
    internal struct Enumerator(KraidArmHitboxSequence sequence)
    {
        /// <summary>Index of the next rectangle in the selected slice.</summary>
        private int next;
        /// <summary>Advances while another rectangle remains in the slice.</summary>
        public bool MoveNext() => next++ < sequence.Length;
        /// <summary>Rectangle selected by the most recent successful advance.</summary>
        public KraidArmCollisionHitbox Current => sequence[next - 1];
    }
}
