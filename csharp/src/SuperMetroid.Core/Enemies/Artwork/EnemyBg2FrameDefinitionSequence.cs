namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Bounded indexed BG2 frame definitions generated on demand for loading and extraction.</summary>
internal readonly ref struct EnemyBg2FrameDefinitionSequence
{
    /// <summary>Empty backing span retained by the default value; constructed sequences use the generator.</summary>
    private readonly ReadOnlySpan<EnemyBg2FrameDefinition> stored;
    /// <summary>On-demand definition provider installed by the bounded constructor.</summary>
    private readonly Func<int, EnemyBg2FrameDefinition>? generate;
    /// <summary>Declared number of frame identities addressable through the indexer.</summary>
    public int Length { get; }

    /// <summary>Creates a counted definition view without materializing all frame records.</summary>
    internal EnemyBg2FrameDefinitionSequence(int count, Func<int, EnemyBg2FrameDefinition> generate)
    {
        stored = default;
        this.generate = generate;
        Length = count;
    }

    /// <summary>Generates a frame only after checking its index against the declared bound.</summary>
    public EnemyBg2FrameDefinition this[int index]
    {
        get
        {
            if ((uint)index >= Length) throw new IndexOutOfRangeException();
            return generate is null ? stored[index] : generate(index);
        }
    }

    /// <summary>A new array of <paramref name="selector"/>'s results for each frame and its index, in frame order.</summary>
    public TResult[] SelectToArray<TResult>(Func<EnemyBg2FrameDefinition, int, TResult> selector)
    {
        Ensure.NotNull(selector);
        var result = new TResult[Length];
        for (int index = 0; index < Length; index++)
            result[index] = selector(this[index], index);
        return result;
    }

    /// <summary>Starts forward traversal over the counted frame identities.</summary>
    public Enumerator GetEnumerator() => new(this);
    /// <summary>Cursor that requests one definition at a time from its parent sequence.</summary>
    internal ref struct Enumerator
    {
        /// <summary>The counted definition view being traversed.</summary>
        private readonly EnemyBg2FrameDefinitionSequence sequence;
        /// <summary>Current zero-based position, initially before the first frame.</summary>
        private int index;
        /// <summary>Places the cursor before the first frame of the supplied sequence.</summary>
        internal Enumerator(EnemyBg2FrameDefinitionSequence sequence)
        {
            this.sequence = sequence;
            index = -1;
        }
        /// <summary>Advances once and reports whether the new position is inside the declared bound.</summary>
        public bool MoveNext() => ++index < sequence.Length;
        /// <summary>Generates the frame at the cursor's current position.</summary>
        public EnemyBg2FrameDefinition Current => sequence[index];
    }
}
