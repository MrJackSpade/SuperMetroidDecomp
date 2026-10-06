using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

/// <summary>One converted gameplay update from <c>tools/convert-smv-updates.py</c>.</summary>
internal readonly record struct ConvertedMovieUpdate(
    int Update, int SourceFrame, ushort Input, string Kind, string TimingClass,
    int ExpectedRecord, int ExcludedNmiAfter, ushort? HardwareWaitLatch);

/// <summary>
/// Read-only view of a converted SMV replay manifest plus its forward-only native
/// WRAM checkpoint stream. Each retained update maps to the native controller-read
/// boundary that follows it; the last record is the terminal state.
/// </summary>
internal sealed class NativeMovieCheckpoints : IDisposable
{
    public const int WorkRamByteCount = 0x20000;
    private const int RecordHeaderBytes = 8;

    private readonly FileStream file;
    private readonly GZipStream trace;
    private readonly byte[] record = new byte[RecordHeaderBytes + WorkRamByteCount];
    private int lastRecord = -1;

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

    public IReadOnlyList<ConvertedMovieUpdate> Updates { get; }
    public int SourceFrameCount { get; }
    public ushort InitialInput { get; }
    /// <summary>Record preceding the first port update; nonzero after a folded power-on prelude.</summary>
    public int InitialRecord { get; }

    /// <summary>Opens a converted replay after checking its format and source-movie identity.</summary>
    public static NativeMovieCheckpoints Open(string directory, byte[] movie)
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "updates.json")));
        JsonElement root = manifest.RootElement;
        string format = root.GetProperty("format").GetString()!;
        if (format != "super-metroid-gameplay-updates-v4")
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
                ? null : (ushort)update.GetProperty("hardwareWaitLatch").GetInt32())).ToArray();
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

    public void Dispose()
    {
        trace.Dispose();
        file.Dispose();
    }
}
