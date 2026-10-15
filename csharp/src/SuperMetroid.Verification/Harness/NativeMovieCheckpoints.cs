using System.Security.Cryptography;
using System.Text.Json;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Read-only view of a converted SMV replay manifest plus its forward-only native WRAM
/// checkpoints. Each retained update maps to the native controller-read boundary that follows
/// it; the last record is the terminal state.
/// </summary>
internal sealed class NativeMovieCheckpoints : IDisposable
{
    /// <summary>Compact replay trace supplying the native WRAM checkpoint recorded at each update boundary.</summary>
    private readonly ReplayTrace trace;
    /// <summary>Index of the last checkpoint consumed from the compressed stream.</summary>
    private int lastRecord = -1;

    /// <summary>Verifies the checkpoint stream against the manifest before opening it for sequential reads.</summary>
    private NativeMovieCheckpoints(ReplayTrace trace, ConvertedMovieUpdate[] updates)
    {
        this.trace = trace;
        Updates = updates;
        JsonElement root = trace.Updates;
        SourceFrameCount = root.GetProperty("sourceFrameCount").GetInt32();
        InitialInput = (ushort)root.GetProperty("initialInput").GetInt32();
        InitialRecord = root.GetProperty("initialRecord").GetInt32();
    }

    /// <summary>Input dispatches and their native checkpoint mapping in replay order.</summary>
    public IReadOnlyList<ConvertedMovieUpdate> Updates { get; }
    /// <summary>Original SMV frame count used to identify the terminal checkpoint.</summary>
    public int SourceFrameCount { get; }
    /// <summary>Controller value already sampled before the first converted gameplay update.</summary>
    public ushort InitialInput { get; }
    /// <summary>Record preceding the first port update; nonzero after a folded power-on prelude.</summary>
    public int InitialRecord { get; }

    /// <summary>
    /// Opens a committed compact trace (<see cref="ReplayTraceMemoryCatalog.MovieDesync"/> layout)
    /// after checking its identities, its format and its source-movie identity.
    /// </summary>
    /// <param name="directory">The compact trace directory.</param>
    /// <param name="movie">The supplied movie's bytes.</param>
    public static NativeMovieCheckpoints Open(string directory, byte[] movie) =>
        Open(ReplayTrace.OpenCompact(directory, ReplayTraceMemoryCatalog.MovieDesync), movie);

    /// <summary>
    /// Opens a private full native capture, whose checkpoints hold complete WRAM; for diagnostics
    /// that need more than the compact layout.
    /// </summary>
    /// <param name="directory">The full capture directory.</param>
    /// <param name="movie">The supplied movie's bytes.</param>
    public static NativeMovieCheckpoints OpenFullCapture(string directory, byte[] movie) =>
        Open(ReplayTrace.OpenFullCapture(directory), movie);

    /// <summary>Binds an opened trace to the supplied movie, disposing the trace if the binding fails.</summary>
    private static NativeMovieCheckpoints Open(ReplayTrace trace, byte[] movie)
    {
        try
        {
            return new NativeMovieCheckpoints(trace, ReadUpdates(trace.Updates, movie));
        }
        catch
        {
            trace.Dispose();
            throw;
        }
    }

    /// <summary>Reads the converted updates after checking the manifest's format and source-movie identity.</summary>
    private static ConvertedMovieUpdate[] ReadUpdates(JsonElement root, byte[] movie)
    {
        string format = (root.GetProperty("format").GetString() ?? throw new InvalidOperationException("Verification requires root.GetProperty(\"format\").GetString()."));
        // v6 records MessageBox_Routine's return frame, which separates a box's own
        // frames from lag its dispatch runs afterward. v7 splits a dispatch entered
        // straight out of CommonBootSection into its own update.
        if (format != "super-metroid-gameplay-updates-v7")
            throw new InvalidDataException($"Unsupported converted replay format {format}.");
        if (root.GetProperty("movieSha256").GetString() != Convert.ToHexString(SHA256.HashData(movie)))
            throw new InvalidDataException("Converted replay was produced from a different movie.");
        var updates = root.GetProperty("updates").EnumerateArray().Select(update => new ConvertedMovieUpdate(
            update.GetProperty("update").GetInt32(),
            update.GetProperty("sourceFrame").GetInt32(),
            (ushort)update.GetProperty("input").GetInt32(),
            (update.GetProperty("kind").GetString() ?? throw new InvalidOperationException("Verification requires update.GetProperty(\"kind\").GetString().")),
            (update.GetProperty("timingClass").GetString() ?? throw new InvalidOperationException("Verification requires update.GetProperty(\"timingClass\").GetString().")),
            update.GetProperty("expectedRecord") is { ValueKind: not JsonValueKind.Null } expectedRecord
                ? expectedRecord.GetInt32() : null,
            update.GetProperty("excludedNmiAfter").GetInt32(),
            update.GetProperty("hardwareWaitLatch").ValueKind == JsonValueKind.Null
                ? null : (ushort)update.GetProperty("hardwareWaitLatch").GetInt32(),
            update.GetProperty("timingEvidence").GetProperty("doorLoaderCompletedEnemySlots") is
                { ValueKind: not JsonValueKind.Null } completedSlots
                ? completedSlots.GetInt32() : null,
            update.GetProperty("messageBoxStartFrame") is { ValueKind: not JsonValueKind.Null } boxStart
                ? boxStart.GetInt32() : null,
            update.GetProperty("messageBoxEndFrame") is { ValueKind: not JsonValueKind.Null } boxEnd
                ? boxEnd.GetInt32() : null)).ToArray();
        if (updates.Length != root.GetProperty("updateCount").GetInt32())
            throw new InvalidDataException("Converted update count disagrees with its update list.");
        return updates;
    }

    /// <summary>
    /// Returns native WRAM expected after <paramref name="update"/> converted updates, or
    /// null when that update ends without a native checkpoint (see
    /// <see cref="ConvertedMovieUpdate.ExpectedRecord"/>). Calls must move forward; skipped
    /// records belong to normalized hardware waits. The update-zero record is complete WRAM.
    /// </summary>
    public NativeCheckpointMemory? ReadAfter(int update)
    {
        if (update != 0 && Updates[update - 1].ExpectedRecord is null)
            return null;
        int wanted = update == 0 ? InitialRecord : (Updates[update - 1].ExpectedRecord ?? throw new InvalidOperationException("Verification requires Updates[update - 1].ExpectedRecord."));
        if (wanted <= lastRecord)
            throw new InvalidOperationException("Native checkpoints are forward-only.");
        NativeCheckpoint record = trace.Records.Read(wanted);
        lastRecord = wanted;
        bool terminal = update == Updates.Count;
        int expectedFrame = terminal ? SourceFrameCount : Updates[update].SourceFrame;
        if (record.SourceFrame != expectedFrame || record.BoundaryPc != (terminal ? 0 : MovieDesyncMemory.ControllerReadBoundary))
            throw new InvalidDataException(
                $"Native checkpoint {wanted} is frame {record.SourceFrame} pc {record.BoundaryPc:X6}; expected frame {expectedFrame}.");
        return record.Memory;
    }

    /// <summary>Confirms the terminal record was the final checkpoint in the stream.</summary>
    public void AssertExhausted()
    {
        if (!trace.Records.IsExhausted())
            throw new InvalidDataException("Native checkpoint stream continues after the movie's terminal state.");
    }

    /// <summary>Closes the decompressor and its underlying checkpoint file.</summary>
    public void Dispose() => trace.Dispose();
}
