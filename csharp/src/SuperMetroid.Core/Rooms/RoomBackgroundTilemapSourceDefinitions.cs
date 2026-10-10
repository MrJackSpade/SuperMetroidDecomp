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

    /// <summary>Read-only, ascending view of distinct source addresses used by library-background decompression commands.</summary>
    private sealed class SourceSequence : IReadOnlyList<int>
    {
        /// <summary>Gets the number of distinct decompressed tilemap sources in the command catalog.</summary>
        public int Count
        {
            get
            {
                int count = 0;
                foreach (int _ in this) count++;
                return count;
            }
        }

        /// <summary>Gets the source address at an ascending, zero-based position in the distinct source view.</summary>
        /// <param name="index">Zero-based position among the sorted source addresses.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative or greater than or equal to <see cref="Count"/>.</exception>
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

        /// <summary>Enumerates each distinct decompressed tilemap source address once, in ascending order.</summary>
        /// <returns>An enumerator over the derived source identities.</returns>
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

        /// <summary>Returns a non-generic enumerator over this sequence.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
