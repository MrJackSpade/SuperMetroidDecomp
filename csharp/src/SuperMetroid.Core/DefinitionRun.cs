namespace SuperMetroid.Core;

/// <summary>
/// A run of one definition entry stated once with how many times it follows itself (#2557): a path, cadence, duration
/// list or any step data that holds the same entry several times in a row names the entry and its count, so the run reads
/// as one fact and cannot drift entry by entry.
/// </summary>
internal static class DefinitionRun
{
    /// <summary><paramref name="entry"/> written <paramref name="count"/> times in a row.</summary>
    /// <typeparam name="T">The entry's type.</typeparam>
    /// <param name="entry">The entry the run repeats.</param>
    /// <param name="count">How many times in a row the entry is written; at least one.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is below one.</exception>
    internal static T[] Repeated<T>(T entry, int count)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);
        var run = new T[count];
        Array.Fill(run, entry);
        return run;
    }
}
