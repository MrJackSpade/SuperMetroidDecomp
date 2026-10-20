namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Ordered physical component view with calculated hitbox selectors.</summary>
internal readonly record struct KraidArmComponentSequence(int Pose, int Length)
{
    /// <summary>Resolves one component offset and hitbox-list identity for the normalized pose.</summary>
    internal KraidArmCollisionComponent this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Length) throw new ArgumentOutOfRangeException(nameof(index));
            return KraidArmCollisionDefinitions.Component(Pose, index);
        }
    }
    /// <summary>Enumerates the selected physical components without allocating an array.</summary>
    public Enumerator GetEnumerator() => new(this);
    /// <summary>Advances through the normalized pose's component order.</summary>
    internal struct Enumerator(KraidArmComponentSequence sequence)
    {
        /// <summary>Index of the next component to resolve.</summary>
        private int next;
        /// <summary>Advances while another component remains in the pose.</summary>
        public bool MoveNext() => next++ < sequence.Length;
        /// <summary>Component returned by the most recent successful advance.</summary>
        public KraidArmCollisionComponent Current => sequence[next - 1];
    }
}
