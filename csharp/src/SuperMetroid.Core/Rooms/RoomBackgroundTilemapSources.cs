namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Distinct compressed visual tilemap sources selected by the bank-$8F library-background
/// commands. This view derives the source index; it does not dispose of the independently
/// required command operands or installed artwork payloads.
/// </summary>
public static class RoomBackgroundTilemapSources
{
    /// <summary>Sorted, immutable source identities, derived without storing a second source lookup.</summary>
    public static IReadOnlyList<int> All { get; } = new SourceSequence();

    /// <summary>Whether a native decompression command selects the requested source identity.</summary>
    public static bool Contains(int sourceAddress)
    {
        foreach (LibraryBackgroundProgram program in LibraryBackgroundProgramDefinitions.All)
        foreach (LibraryBackgroundInstruction instruction in program.Instructions)
            if (instruction.Command == LibraryBackgroundCommand.DecompressToWorkRam &&
                instruction.SourceAddress == sourceAddress)
                return true;
        return false;
    }

    /// <summary>Derives sorted unique decompression sources directly from compiled library-background commands.</summary>
    private sealed class SourceSequence : IReadOnlyList<int>
    {
        /// <summary>Counts distinct tilemap sources without retaining a parallel source table.</summary>
        public int Count
        {
            get
            {
                int count = 0;
                foreach (int _ in this) count++;
                return count;
            }
        }

        /// <summary>Returns the indexed source from the sorted unique enumeration, rejecting missing ordinals.</summary>
        public int this[int index]
        {
            get
            {
                Ensure.AtLeastZero(index);
                int current = 0;
                foreach (int source in this)
                    if (current++ == index) return source;
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>Repeatedly finds the next greater decompression address across all compiled programs.</summary>
        public IEnumerator<int> GetEnumerator()
        {
            int previous = int.MinValue;
            while (true)
            {
                int? next = null;
                foreach (LibraryBackgroundProgram program in LibraryBackgroundProgramDefinitions.All)
                foreach (LibraryBackgroundInstruction instruction in program.Instructions)
                {
                    if (instruction.Command != LibraryBackgroundCommand.DecompressToWorkRam ||
                        instruction.SourceAddress <= previous)
                        continue;
                    if (next is null || instruction.SourceAddress < next.Value)
                        next = instruction.SourceAddress;
                }
                if (next is null) yield break;
                previous = next.Value;
                yield return previous;
            }
        }

        /// <summary>Exposes the same sorted source identities to non-generic consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
