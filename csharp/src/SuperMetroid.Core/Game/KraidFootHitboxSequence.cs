namespace SuperMetroid.Core.Game;

/// <summary>Direct construction of the single shared rectangle without a stored hitbox lookup.</summary>
internal readonly record struct KraidFootHitboxSequence(int Length)
{
    /// <summary>Constructs the shared centered rectangle and its background-touch/no-op-shot callbacks.</summary>
    internal KraidFootCollisionHitbox this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Length) throw new IndexOutOfRangeException();
            return new(-6, -6, 6, 6, EnemyAiCodePointers.BankA7.KraidBackgroundTouch,
                EnemyAiCodePointers.BankA7.KraidNoOpShot);
        }
    }
    /// <summary>Enumerates the one shared Kraid foot hitbox.</summary>
    public Enumerator GetEnumerator() => new(this);
    /// <summary>Cursor over the shared physical rectangle.</summary>
    internal struct Enumerator(KraidFootHitboxSequence sequence)
    {
        /// <summary>Index of the next rectangle to expose.</summary>
        private int next;
        /// <summary>Advances once for the one-rectangle hitbox list.</summary>
        public bool MoveNext() => next++ < sequence.Length;
        /// <summary>The one shared rectangle after the cursor advances.</summary>
        public KraidFootCollisionHitbox Current => sequence[next - 1];
    }
}
