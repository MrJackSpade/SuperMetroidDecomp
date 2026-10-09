using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

/// <summary>One retained gameplay update and its mapping to source-movie timing and a native WRAM checkpoint.</summary>
/// <param name="Update">One-based port update number represented by this record.</param>
/// <param name="SourceFrame">Source SMV frame whose controller input is consumed by this update.</param>
/// <param name="Input">Controller word supplied at the converted update boundary.</param>
/// <param name="Kind">Converter classification for the update's source-frame behavior.</param>
/// <param name="TimingClass">Timing category used to explain how source frames map to this gameplay update.</param>
/// <param name="ExpectedRecord">Index of the native checkpoint expected after this update.</param>
/// <param name="ExcludedNmiAfter">Count of source NMI frames excluded after this update as normalized hardware wait.</param>
/// <param name="HardwareWaitLatch">Optional native latch value identifying a hardware-wait interval.</param>
/// <param name="DoorLoaderCompletedEnemySlots">Optional count of enemy slots completed by the door loader as timing evidence.</param>
/// <param name="MessageBoxStartFrame">Optional source frame at which a message-box routine begins.</param>
/// <param name="MessageBoxEndFrame">Optional source frame at which the message-box routine returns.</param>
internal readonly record struct ConvertedMovieUpdate(
    int Update, int SourceFrame, ushort Input, string Kind, string TimingClass,
    int ExpectedRecord, int ExcludedNmiAfter, ushort? HardwareWaitLatch, int? DoorLoaderCompletedEnemySlots,
    int? MessageBoxStartFrame, int? MessageBoxEndFrame);

/// <summary>
/// Read-only view of a converted SMV replay manifest plus its forward-only native
/// WRAM checkpoint stream. Each retained update maps to the native controller-read
/// boundary that follows it; the last record is the terminal state.
/// </summary>
internal sealed class NativeMovieCheckpoints : IDisposable
{
    /// <summary>Number of bytes in a complete native WRAM snapshot.</summary>
    public const int WorkRamByteCount = 0x20000;
    /// <summary>Bytes preceding each checkpoint's WRAM payload: little-endian frame number and program counter.</summary>
    private const int RecordHeaderBytes = 8;

    /// <summary>Compressed checkpoint archive opened from the replay directory.</summary>
    private readonly FileStream file;
    /// <summary>Forward-only decompressor for native checkpoint records.</summary>
    private readonly GZipStream trace;
    /// <summary>Reusable buffer holding one record header and WRAM payload.</summary>
    private readonly byte[] record = new byte[RecordHeaderBytes + WorkRamByteCount];
    /// <summary>Index of the most recently consumed record, or minus one before the first read.</summary>
    private int lastRecord = -1;

    /// <summary>Creates a checkpoint reader after the manifest identity has been validated.</summary>
    /// <param name="directory">Directory containing the update manifest and compressed checkpoint stream.</param>
    /// <param name="root">Cloned manifest object whose metadata remains available after its document is disposed.</param>
    /// <param name="updates">Converted update mapping used to select and validate checkpoint records.</param>
    private NativeMovieCheckpoints(string directory, JsonElement root, ConvertedMovieUpdate[] updates)
    {
        Updates = updates;
        SourceFrameCount = root.GetProperty("sourceFrameCount").GetInt32();
        InitialInput = (ushort)root.GetProperty("initialInput").GetInt32();
        InitialRecord = root.GetProperty("initialRecord").GetInt32();
        file = File.OpenRead(Path.Combine(directory, "update-boundaries.wram.gz"));
        string checkpointHash = Convert.ToHexString(SHA256.HashData(file));
        if (checkpointHash != root.GetProperty("checkpointsSha256").GetString())
            throw new InvalidDataException("Native checkpoint stream does not match the converted manifest.");
        file.Position = 0;
        trace = new GZipStream(file, CompressionMode.Decompress);
    }

    /// <summary>Ordered source-frame and input mapping for every retained port update.</summary>
    public IReadOnlyList<ConvertedMovieUpdate> Updates { get; }
    /// <summary>Source movie's terminal frame number used to validate the final checkpoint.</summary>
    public int SourceFrameCount { get; }
    /// <summary>Controller word applied once before the first converted gameplay update.</summary>
    public ushort InitialInput { get; }
    /// <summary>Record preceding the first port update; nonzero after a folded power-on prelude.</summary>
    public int InitialRecord { get; }

    /// <summary>Opens a converted replay after checking its format and source-movie identity.</summary>
    public static NativeMovieCheckpoints Open(string directory, byte[] movie)
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "updates.json")));
        JsonElement root = manifest.RootElement;
        string format = root.GetProperty("format").GetString()!;
        // v6 records MessageBox_Routine's return frame, which separates a box's own
        // frames from lag its dispatch runs afterward.
        if (format != "super-metroid-gameplay-updates-v6")
            throw new InvalidDataException($"Unsupported converted replay format {format}.");
        if (root.GetProperty("movieSha256").GetString() != Convert.ToHexString(SHA256.HashData(movie)))
            throw new InvalidDataException("Converted replay was produced from a different movie.");
        var updates = root.GetProperty("updates").EnumerateArray().Select(update => new ConvertedMovieUpdate(
            update.GetProperty("update").GetInt32(),
            update.GetProperty("sourceFrame").GetInt32(),
            (ushort)update.GetProperty("input").GetInt32(),
            update.GetProperty("kind").GetString()!,
            update.GetProperty("timingClass").GetString()!,
            update.GetProperty("expectedRecord").GetInt32(),
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
        return new NativeMovieCheckpoints(directory, root.Clone(), updates);
    }

    /// <summary>
    /// Returns native WRAM expected after <paramref name="update"/> converted updates.
    /// Calls must move forward; skipped records belong to normalized hardware waits.
    /// </summary>
    public byte[] ReadAfter(int update)
    {
        int wanted = update == 0 ? InitialRecord : Updates[update - 1].ExpectedRecord;
        if (wanted <= lastRecord)
            throw new InvalidOperationException("Native checkpoints are forward-only.");
        while (lastRecord < wanted)
        {
            trace.ReadExactly(record);
            lastRecord++;
        }
        int frame = BinaryPrimitives.ReadInt32LittleEndian(record);
        int pc = BinaryPrimitives.ReadInt32LittleEndian(record.AsSpan(4));
        bool terminal = update == Updates.Count;
        int expectedFrame = terminal ? SourceFrameCount : Updates[update].SourceFrame;
        if (frame != expectedFrame || pc != (terminal ? 0 : MovieDesyncMemory.ControllerReadBoundary))
            throw new InvalidDataException(
                $"Native checkpoint {wanted} is frame {frame} pc {pc:X6}; expected frame {expectedFrame}.");
        return record.AsSpan(RecordHeaderBytes).ToArray();
    }

    /// <summary>Confirms the terminal record was the final checkpoint in the stream.</summary>
    public void AssertExhausted()
    {
        if (trace.ReadByte() != -1)
            throw new InvalidDataException("Native checkpoint stream continues after the movie's terminal state.");
    }

    /// <summary>Closes the decompression stream and its underlying checkpoint archive.</summary>
    public void Dispose()
    {
        trace.Dispose();
        file.Dispose();
    }
}
