using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Audits the native snapshot import against a moment the 100% movie reaches by replay.
    /// The replay matches the cartridge through every compared update, so its production
    /// state is the reference: importing the same update's native WRAM must reproduce it.
    /// Each listed difference names state the import does not yet carry.
    /// </summary>
    /// <param name="traceDirectory">The converted 100% movie trace.</param>
    /// <param name="update">The converted update after which the import is audited.</param>
    private static void AuditSnapshotImport(string traceDirectory, int update)
    {
        var movie = ReplayMovie.Load("100%", FullPlaythroughMoviePath, FullPlaythroughMovieSha256);
        using var checkpoints = NativeMovieCheckpoints.Open(traceDirectory, movie.Bytes);
        var (replayed, memory, _) = ReplayConvertedMovie(movie, checkpoints, traceFromUpdate: null, update);
        byte[] saveRam = PrivateState.Field<SuperMetroidAddressSpace>(replayed, "bus").SaveRam.ToArray();
        SuperMetroidGame imported = ImportNativeSnapshot(memory, saveRam).Game;
        IReadOnlyList<string> differences = PortStateDiff.Compare(replayed, imported, "game",
            path => path.EndsWith(".bus", StringComparison.Ordinal));
        foreach (string difference in differences)
            Console.WriteLine(difference);
        Console.WriteLine($"Snapshot import audit after update {update}: {differences.Count} differences.");
    }
}
