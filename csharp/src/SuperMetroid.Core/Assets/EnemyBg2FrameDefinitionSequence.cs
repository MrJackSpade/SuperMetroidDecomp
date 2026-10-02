namespace SuperMetroid.Core.Assets;

/// <summary>Indexed BG2 definitions supplied either by existing spans or by a
/// bounded generator. Loading and extraction consume entries without caching a
/// generated definition lookup.</summary>
internal readonly ref struct EnemyBg2FrameDefinitionSequence
{
    private readonly ReadOnlySpan<EnemyBg2FrameDefinition> stored;
    private readonly Func<int, EnemyBg2FrameDefinition>? generate;
    public int Length { get; }

    internal EnemyBg2FrameDefinitionSequence(int count, Func<int, EnemyBg2FrameDefinition> generate)
    {
        stored = default;
        this.generate = generate;
        Length = count;
    }

    private EnemyBg2FrameDefinitionSequence(ReadOnlySpan<EnemyBg2FrameDefinition> stored)
    {
        this.stored = stored;
        generate = null;
        Length = stored.Length;
    }

    public EnemyBg2FrameDefinition this[int index]
    {
        get
        {
            if ((uint)index >= Length) throw new IndexOutOfRangeException();
            return generate is null ? stored[index] : generate(index);
        }
    }

    public static implicit operator EnemyBg2FrameDefinitionSequence(ReadOnlySpan<EnemyBg2FrameDefinition> value) => new(value);
    public static implicit operator EnemyBg2FrameDefinitionSequence(EnemyBg2FrameDefinition[] value) => new(value);

    internal EnemyBg2FrameDefinition[] ToArray()
    {
        var result = new EnemyBg2FrameDefinition[Length];
        for (int index = 0; index < result.Length; index++) result[index] = this[index];
        return result;
    }

    public Enumerator GetEnumerator() => new(this);
    internal ref struct Enumerator
    {
        private readonly EnemyBg2FrameDefinitionSequence sequence;
        private int index;
        internal Enumerator(EnemyBg2FrameDefinitionSequence sequence)
        {
            this.sequence = sequence;
            index = -1;
        }
        public bool MoveNext() => ++index < sequence.Length;
        public EnemyBg2FrameDefinition Current => sequence[index];
    }
}
