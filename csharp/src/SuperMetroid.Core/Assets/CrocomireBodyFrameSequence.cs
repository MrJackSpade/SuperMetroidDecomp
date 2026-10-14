namespace SuperMetroid.Core.Assets;

/// <summary>Calculated body-frame enumeration; no cached pointer lookup.</summary>
internal readonly record struct CrocomireBodyFrameSequence(int Length)
{
    /// <summary>Calculates one selected fight-body root from its sequence index.</summary>
    internal ushort this[int index] => CrocomireBodyVisualDefinitions.FramePointer(index);
    /// <summary>Walks native body roots in selected instruction order.</summary>
    public Enumerator GetEnumerator() => new(this);
    /// <summary>Value-type cursor over the calculated frame sequence.</summary>
    internal struct Enumerator(CrocomireBodyFrameSequence sequence)
    {
        /// <summary>Ordinal before the next move; starts before the first frame.</summary>
        private int index = -1;
        /// <summary>Advances until the fifty selected body roots are exhausted.</summary>
        public bool MoveNext() => ++index < sequence.Length;
        /// <summary>Native pointer for the current selected fight-body frame.</summary>
        public ushort Current => sequence[index];
    }
}
