namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Two physical components calculated on access; an unsuccessful lookup returns an empty sequence.</summary>
internal readonly record struct KraidFootComponentSequence(int Frame, int Length)
{
    /// <summary>Calculates the fixed or moving component's offset with their shared rectangle.</summary>
    internal KraidFootCollisionComponent this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Length) throw new ArgumentOutOfRangeException(nameof(index));
            return index == 0
                ? new(new(KraidFootCollisionDefinitions.FirstX(Frame), KraidFootCollisionDefinitions.FirstY(Frame)), KraidFootCollisionDefinitions.SharedHitboxes)
                : new(new(KraidFootCollisionDefinitions.SecondX(Frame), KraidFootCollisionDefinitions.SecondY(Frame)), KraidFootCollisionDefinitions.SharedHitboxes);
        }
    }
    /// <summary>Enumerates the calculated components without a stored frame array.</summary>
    public Enumerator GetEnumerator() => new(this);
    /// <summary>Cursor over Kraid foot physical components.</summary>
    internal struct Enumerator(KraidFootComponentSequence sequence)
    {
        /// <summary>Index of the next component in the two-part frame.</summary>
        private int next;
        /// <summary>Advances through the frame's bounded component sequence.</summary>
        public bool MoveNext() => next++ < sequence.Length;
        /// <summary>Most recently selected fixed or moving foot component.</summary>
        public KraidFootCollisionComponent Current => sequence[next - 1];
    }
}
