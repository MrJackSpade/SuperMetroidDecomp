namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The pass's graphics chunks in copy order, each landing one scratch stride after the last.
/// The second pass's seventh chunk is palette and adjacent data rather than tiles; it is
/// preserved, as is the caller's inclusive $0201-word copy.</summary>
internal readonly record struct CrocomireMeltingCopySequence(CrocomireMeltingPass Pass)
{
    /// <summary>Six first-pass or seven second-pass source/scratch pairs.</summary>
    internal int Length => Pass.ChunkCount;
    /// <summary>Names one chunk of the pass and calculates its scratch destination.</summary>
    internal CrocomireMeltingCopy this[int index]
    {
        get
        {
            if ((uint)index >= Length) throw new IndexOutOfRangeException();
            CrocomireMeltingHeader pass = Pass.HeaderOffset == (ushort)CrocomireMeltingHeader.First
                ? CrocomireMeltingHeader.First
                : CrocomireMeltingHeader.Second;
            return new(pass, index,
                CrocomireMeltingTransferDefinitions.ScratchStart + index * CrocomireMeltingTransferDefinitions.ChunkByteStride);
        }
    }

    /// <summary>Traverses copy pairs in the native melt-table order.</summary>
    public Enumerator GetEnumerator() => new(this);
    /// <summary>Value enumerator over calculated source-to-scratch copies.</summary>
    internal struct Enumerator(CrocomireMeltingCopySequence sequence)
    {
        /// <summary>Current zero-based copy ordinal, initially before the first entry.</summary>
        private int index = -1;
        /// <summary>Advances until every chunk of the selected pass has been visited.</summary>
        public bool MoveNext() => ++index < sequence.Length;
        /// <summary>Calculates the native copy pair at the current ordinal.</summary>
        public CrocomireMeltingCopy Current => sequence[index];
    }
}
