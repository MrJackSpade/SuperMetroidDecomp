using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Frontend;
using static SuperMetroid.TestSupport.MovieReplay.NativeSnapshotImport;
namespace SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Audio;

/// <summary>Compares native snapshot import with the same 100% movie update reached through production replay.</summary>
internal static class SnapshotImportAuditVerification
{
    /// <summary>
    /// Audits the native snapshot import against a moment the 100% movie reaches by replay.
    /// The replay matches the cartridge through every compared update, so its production
    /// state is the reference: importing the same update's native WRAM must reproduce it.
    /// Each listed difference names state the import does not yet carry.
    /// </summary>
    /// <param name="traceDirectory">The converted 100% movie's private full native capture (complete WRAM at every checkpoint).</param>
    /// <param name="update">The converted update after which the import is audited.</param>
    internal static void AuditSnapshotImport(string traceDirectory, int update)
    {
        var (session, timeline) = ConvertedMovieSession.OpenFullCapture(ReplayMovieId.Full100, traceDirectory, update);
        using (session)
            HeadlessMovieReplay.RunToEnd(session);
        SuperMetroidGame replayed = session.Game;
        NativeCheckpointMemory memory = timeline.Memory;
        byte[] saveRam = SuperMetroid.Diagnostics.Saves.SaveRamCodec.Encode(replayed.Saves, SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps);
        SuperMetroidGame imported = ImportNativeSnapshot(memory.ToWorkRam(), saveRam).Game;
        IReadOnlyList<string> differences = PortStateDiff.Compare(replayed, imported, "game",
            path => path.EndsWith(".bus", StringComparison.Ordinal));
        foreach (string difference in differences)
            Console.WriteLine(difference);
        Console.WriteLine($"Snapshot import audit after update {update}: {differences.Count} differences.");
    }

    /// <summary>Runs the snapshot comparison at the requested converted update and prints any state differences.</summary>
    [VerificationCommand("--snapshot-import-audit", "auditTraceDirectory", "auditUpdate")]
    internal static int SnapshotImportAuditCommand(string auditTraceDirectory, string auditUpdate)
    {
     AuditSnapshotImport(auditTraceDirectory, int.Parse(auditUpdate)); return 0; 
    }
}
