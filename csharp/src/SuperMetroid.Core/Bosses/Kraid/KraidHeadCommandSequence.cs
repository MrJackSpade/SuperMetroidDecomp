namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Calculated native head commands with no stored lookup or startup cache.</summary>
internal readonly record struct KraidHeadCommandSequence(int Length) : IEnumerable<KraidHeadInstructionDefinition>
{
    /// <summary>LINQ traversal of the calculated commands; boxes one enumerator.</summary>
    IEnumerator<KraidHeadInstructionDefinition> IEnumerable<KraidHeadInstructionDefinition>.GetEnumerator()
    {
        for (int index = 0; index < Length; index++)
            yield return this[index];
    }

    /// <summary>Non-generic traversal of the calculated commands.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => ((IEnumerable<KraidHeadInstructionDefinition>)this).GetEnumerator();

    /// <summary>Calculates a command by its ordinal in native program order.</summary>
    public KraidHeadInstructionDefinition this[int index] => KraidHeadInstructionDefinitions.Command(index);
    /// <summary>Materializes all calculated commands for consumers requiring an array.</summary>
    public KraidHeadInstructionDefinition[] ToArray()
    {
        var result = new KraidHeadInstructionDefinition[Length];
        for (int index = 0; index < result.Length; index++) result[index] = this[index];
        return result;
    }
    /// <summary>Traverses native commands without allocating a backing table.</summary>
    public Enumerator GetEnumerator() => new(this);
    /// <summary>Value enumerator over a calculated Kraid head command sequence.</summary>
    public struct Enumerator(KraidHeadCommandSequence sequence)
    {
        /// <summary>Ordinal of the command to expose on the next iteration.</summary>
        private int next;
        /// <summary>Advances until all commands in the sequence have been visited.</summary>
        public bool MoveNext() => next++ < sequence.Length;
        /// <summary>Calculates the command selected by the most recent advance.</summary>
        public KraidHeadInstructionDefinition Current => KraidHeadInstructionDefinitions.Command(next - 1);
    }
}
