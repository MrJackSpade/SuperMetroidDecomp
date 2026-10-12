namespace SuperMetroid.Core.Assets;

/// <summary>Indexed BG2 definitions supplied either by existing spans or by a
/// bounded generator. Loading and extraction consume entries without caching a
/// generated definition lookup.</summary>
internal readonly ref struct EnemyBg2FrameDefinitionSequence
{
    /// <summary>Stores stored.</summary>
    private readonly ReadOnlySpan<EnemyBg2FrameDefinition> stored;
    /// <summary>Stores generate.</summary>
    private readonly Func<int, EnemyBg2FrameDefinition>? generate;
    /// <summary>Gets length.</summary>
    public int Length { get; }

    /// <summary>Initializes a new enemy bg2 frame definition sequence instance.</summary>
    internal EnemyBg2FrameDefinitionSequence(int count, Func<int, EnemyBg2FrameDefinition> generate)
    {
        stored = default;
        this.generate = generate;
        Length = count;
    }

    /// <summary>Gets or sets an indexed value.</summary>
    public EnemyBg2FrameDefinition this[int index]
    {
        get
        {
            if ((uint)index >= Length) throw new IndexOutOfRangeException();
            return generate is null ? stored[index] : generate(index);
        }
    }

    /// <summary>Gets enumerator.</summary>
    public Enumerator GetEnumerator() => new(this);
    /// <summary>Represents enumerator.</summary>
    internal ref struct Enumerator
    {
        /// <summary>Stores sequence.</summary>
        private readonly EnemyBg2FrameDefinitionSequence sequence;
        /// <summary>Stores index.</summary>
        private int index;
        /// <summary>Initializes a new enumerator instance.</summary>
        internal Enumerator(EnemyBg2FrameDefinitionSequence sequence)
        {
            this.sequence = sequence;
            index = -1;
        }
        /// <summary>Moves next.</summary>
        public bool MoveNext() => ++index < sequence.Length;
        /// <summary>Gets current.</summary>
        public EnemyBg2FrameDefinition Current => sequence[index];
    }
}
