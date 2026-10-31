namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>The rotting-heap bytes the cartridge keeps one corpse's rot schedule in (#1472).</summary>
/// <param name="Start">The first byte.</param>
/// <param name="End">The byte after the last.</param>
internal readonly record struct CorpseScheduleHeapBytes(int Start, int End)
{
    /// <summary>Whether this schedule and <paramref name="other"/> share any heap byte.</summary>
    internal bool Overlaps(CorpseScheduleHeapBytes other) => Start < other.End && other.Start < End;
}
