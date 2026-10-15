namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Native copy pairs: consecutive $0200-byte source/scratch strides.
/// The second pass includes $A4:B87D (palette and adjacent data) as its seventh
/// source. Preserve that native chunk and the caller's inclusive $0201-word copy.</summary>
internal readonly record struct CrocomireMeltingCopySequence(CrocomireMeltingPass Pass)
{
    /// <summary>Six first-pass or seven second-pass source/scratch pairs.</summary>
    internal int Length => Pass.ChunkCount;
    /// <summary>Calculates one $0200-stride source address and its scratch destination.</summary>
    internal CrocomireMeltingCopy this[int index]
    {
        get
        {
            if ((uint)index >= Length) throw new IndexOutOfRangeException();
            int source = Pass.HeaderOffset == (ushort)CrocomireMeltingHeader.First ? 0xa07d : 0xac7d;
            return new((ushort)(source + index * 0x200), (ushort)(CrocomireMeltingTransferDefinitions.ScratchStart + index * 0x200));
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
