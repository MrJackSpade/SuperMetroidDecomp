namespace SuperMetroid.Core.Assets;

/// <summary>Indexed BG2 definitions supplied either by existing spans or by a
/// bounded generator. Loading and extraction consume entries without caching a
/// generated definition lookup.</summary>
internal readonly ref struct EnemyBg2FrameDefinitionSequence
{
    /// <summary>Precomputed frame definitions used when the sequence is backed by an existing span.</summary>
    private readonly ReadOnlySpan<EnemyBg2FrameDefinition> stored;
    /// <summary>Optional indexed factory used to produce a frame definition on demand.</summary>
    private readonly Func<int, EnemyBg2FrameDefinition>? generate;
    /// <summary>Gets the number of valid frame indices in the sequence.</summary>
    public int Length { get; }

    /// <summary>Creates a sequence whose entries are produced on demand for their requested index.</summary>
    /// <param name="count">Number of frame definitions exposed by the sequence.</param>
    /// <param name="generate">Factory that returns the definition for each valid zero-based index.</param>
    internal EnemyBg2FrameDefinitionSequence(int count, Func<int, EnemyBg2FrameDefinition> generate)
    {
        stored = default;
        this.generate = generate;
        Length = count;
    }

    /// <summary>Gets a frame definition by zero-based index, using the generator when one is present.</summary>
    /// <param name="index">Index of the requested frame, from zero through <see cref="Length"/> minus one.</param>
    /// <exception cref="IndexOutOfRangeException">The index is outside the sequence bounds.</exception>
    public EnemyBg2FrameDefinition this[int index]
    {
        get
        {
            if ((uint)index >= Length) throw new IndexOutOfRangeException();
            return generate is null ? stored[index] : generate(index);
        }
    }

    /// <summary>Creates a value-type enumerator positioned before the first frame definition.</summary>
    /// <returns>An enumerator that reads entries from this sequence in index order.</returns>
    public Enumerator GetEnumerator() => new(this);

    /// <summary>Tracks a zero-based traversal of an <see cref="EnemyBg2FrameDefinitionSequence"/>.</summary>
    internal ref struct Enumerator
    {
        /// <summary>Sequence providing the frame definition for the current index.</summary>
        private readonly EnemyBg2FrameDefinitionSequence sequence;
        /// <summary>Index of the current element, initialized to -1 before the first move.</summary>
        private int index;

        /// <summary>Creates an enumerator that has not yet advanced into the supplied sequence.</summary>
        /// <param name="sequence">Sequence to traverse.</param>
        internal Enumerator(EnemyBg2FrameDefinitionSequence sequence)
        {
            this.sequence = sequence;
            index = -1;
        }
        /// <summary>Advances to the next index and reports whether that index is within the sequence.</summary>
        /// <returns><see langword="true"/> when another frame is available; otherwise, <see langword="false"/>.</returns>
        public bool MoveNext() => ++index < sequence.Length;

        /// <summary>Gets the frame definition at the current index.</summary>
        public EnemyBg2FrameDefinition Current => sequence[index];
    }
}
