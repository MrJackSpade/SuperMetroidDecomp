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

    private sealed class SourceSequence : IReadOnlyList<int>
    {
        public int Count
        {
            get
            {
                int count = 0;
                foreach (int _ in this) count++;
                return count;
            }
        }

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

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}