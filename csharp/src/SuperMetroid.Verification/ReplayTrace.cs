using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

/// <summary>One native checkpoint record: the boundary it was captured at and its WRAM.</summary>
/// <param name="SourceFrame">Source movie frame of the capture boundary.</param>
/// <param name="BoundaryPc">Long address of the capture boundary; zero for the terminal record.</param>
/// <param name="Memory">The record's WRAM, complete or limited to a layout.</param>
internal readonly record struct NativeCheckpoint(int SourceFrame, int BoundaryPc, NativeCheckpointMemory Memory);

/// <summary>Forward-only reader over a trace's native checkpoint records, in capture order.</summary>
internal abstract class NativeCheckpointRecords : IDisposable
{
    /// <summary>Bytes before each record's WRAM: little-endian source-frame and boundary-PC words.</summary>
    public const int HeaderBytes = 2 * sizeof(int);

    /// <summary>Index of the last record read, or -1 before the first.</summary>
    private int lastRecord = -1;

    /// <summary>
    /// Advances to record <paramref name="record"/> (zero-based) and returns it. Records between the
    /// previous read and this one are skipped; reading backwards throws.
    /// </summary>
    /// <param name="record">The record index; greater than every index read before.</param>
    public NativeCheckpoint Read(int record)
    {
        if (record <= lastRecord)
            throw new InvalidOperationException("Native checkpoints are forward-only.");
        NativeCheckpoint? result = null;
        while (lastRecord < record)
        {
            lastRecord++;
            result = Advance(lastRecord, lastRecord == record);
        }
        return (result ?? throw new InvalidOperationException("Verification requires result."));
    }

    /// <summary>Whether the stream ends after the last record read.</summary>
    public abstract bool IsExhausted();

    /// <summary>Consumes record <paramref name="index"/>; returns it only when <paramref name="materialize"/>.</summary>
    /// <param name="index">The record being consumed, one past the previous.</param>
    /// <param name="materialize">Whether the caller wants this record's memory.</param>
    protected abstract NativeCheckpoint? Advance(int index, bool materialize);

    /// <summary>Closes the underlying streams.</summary>
    public abstract void Dispose();

    /// <summary>Reads a record header's source frame and boundary PC.</summary>
    /// <param name="header">At least <see cref="HeaderBytes"/> bytes.</param>
    protected static (int Frame, int Pc) ReadHeader(ReadOnlySpan<byte> header) =>
        (BinaryPrimitives.ReadInt32LittleEndian(header), BinaryPrimitives.ReadInt32LittleEndian(header[sizeof(int)..]));
}

/// <summary>
/// The private full native capture stream (<c>update-boundaries.wram.gz</c>): each record is the
/// header followed by the complete 128 KiB WRAM image.
/// </summary>
internal sealed class FullCheckpointRecords : NativeCheckpointRecords
{
    /// <summary>The capture file this reader owns.</summary>
    private readonly FileStream file;
    /// <summary>Decompressing view over the capture file.</summary>
    private readonly GZipStream trace;
    /// <summary>Reused storage for one header and complete WRAM image.</summary>
    private readonly byte[] record = new byte[HeaderBytes + ReplayTraceMemoryCatalog.WorkRamByteCount];

    /// <summary>Reads records from an open capture file, which this reader owns.</summary>
    /// <param name="file">The gzip checkpoint stream, positioned at its start.</param>
    public FullCheckpointRecords(FileStream file)
    {
        this.file = file;
        trace = new GZipStream(file, CompressionMode.Decompress);
    }

    /// <inheritdoc />
    public override bool IsExhausted() => trace.ReadByte() == -1;

    /// <inheritdoc />
    protected override NativeCheckpoint? Advance(int index, bool materialize)
    {
        trace.ReadExactly(record);
        if (!materialize)
            return null;
        var (frame, pc) = ReadHeader(record);
        return new NativeCheckpoint(frame, pc, NativeCheckpointMemory.FromWorkRam(record.AsSpan(HeaderBytes).ToArray()));
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        trace.Dispose();
        file.Dispose();
    }
}

/// <summary>
/// A compact trace's checkpoint stream (<c>expected.bin.gz</c>). Each record is the header followed
/// by the layout's payload XORed with the previous record's payload (the first with zeros): a fixed
/// layout in which unchanged bytes are zero, so the stream compresses to what actually changed.
/// The initial record also exists whole in <c>initial.wram.gz</c>; reading it returns that image.
/// </summary>
internal sealed class CompactCheckpointRecords : NativeCheckpointRecords
{
    /// <summary>The compact stream this reader owns.</summary>
    private readonly FileStream file;
    /// <summary>Decompressing view over the compact stream.</summary>
    private readonly GZipStream trace;
    /// <summary>Ranges each payload holds.</summary>
    private readonly ReplayTraceLayout layout;
    /// <summary>Records the manifest declares.</summary>
    private readonly int recordCount;
    /// <summary>Index of the record also stored whole in the initial file.</summary>
    private readonly int initialRecord;
    /// <summary>The complete initial record returned in place of its payload.</summary>
    private readonly NativeCheckpoint initial;
    /// <summary>Reused storage for one record header.</summary>
    private readonly byte[] header = new byte[HeaderBytes];
    /// <summary>Reused storage for one XORed payload.</summary>
    private readonly byte[] delta;
    /// <summary>The running payload reconstructed from the deltas.</summary>
    private readonly byte[] payload;

    /// <summary>Reads records from an open compact stream, which this reader owns.</summary>
    /// <param name="file">The gzip compact stream, positioned at its start.</param>
    /// <param name="layout">The layout the payloads were packed with.</param>
    /// <param name="counts">The manifest's record count and initial record index.</param>
    /// <param name="initial">The complete initial record.</param>
    public CompactCheckpointRecords(FileStream file, ReplayTraceLayout layout, (int Records, int Initial) counts, NativeCheckpoint initial)
    {
        this.file = file;
        this.layout = layout;
        (recordCount, initialRecord) = counts;
        this.initial = initial;
        trace = new GZipStream(file, CompressionMode.Decompress);
        delta = new byte[layout.PayloadByteCount];
        payload = new byte[layout.PayloadByteCount];
    }

    /// <inheritdoc />
    public override bool IsExhausted() => trace.ReadByte() == -1;

    /// <inheritdoc />
    protected override NativeCheckpoint? Advance(int index, bool materialize)
    {
        if (index >= recordCount)
            throw new InvalidDataException($"The compact replay trace holds {recordCount} records; record {index} does not exist.");
        trace.ReadExactly(header);
        trace.ReadExactly(delta);
        for (int offset = 0; offset < payload.Length; offset++)
            payload[offset] ^= delta[offset];
        var (frame, pc) = ReadHeader(header);
        if (index == initialRecord)
        {
            var packedInitial = new byte[layout.PayloadByteCount];
            layout.Pack(initial.Memory.Slice(0, ReplayTraceMemoryCatalog.WorkRamByteCount), packedInitial);
            if (frame != initial.SourceFrame || pc != initial.BoundaryPc || !packedInitial.AsSpan().SequenceEqual(payload))
                throw new InvalidDataException($"Compact checkpoint {index} disagrees with the trace's complete initial record.");
            return materialize ? initial : null;
        }
        return materialize ? new NativeCheckpoint(frame, pc, NativeCheckpointMemory.FromPayload(layout, payload.ToArray())) : null;
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        trace.Dispose();
        file.Dispose();
    }
}

/// <summary>
/// Identity of a compact replay trace directory (<c>compact-manifest.json</c>): its format, the
/// <see cref="ReplayTraceLayout"/> it was packed with, the source capture it came from and the
/// SHA-256 of each compact file.
/// </summary>
/// <param name="Format">Always <see cref="ReplayTrace.CompactFormat"/>.</param>
/// <param name="Layout">The layout name.</param>
/// <param name="LayoutSha256">The layout's <see cref="ReplayTraceLayout.Sha256"/>.</param>
/// <param name="PayloadBytes">Bytes of WRAM in each record's payload.</param>
/// <param name="RecordCount">Checkpoint records, the initial and terminal records included.</param>
/// <param name="InitialRecord">Index of the record kept whole in <c>initial.wram.gz</c>.</param>
/// <param name="MovieSha256">The source movie's SHA-256, as in <c>updates.json</c>.</param>
/// <param name="SourceCheckpointsSha256">SHA-256 of the full capture stream the trace was compacted from.</param>
/// <param name="UpdatesSha256">SHA-256 of <c>updates.json.gz</c>.</param>
/// <param name="InitialSha256">SHA-256 of <c>initial.wram.gz</c>.</param>
/// <param name="ExpectedSha256">SHA-256 of <c>expected.bin.gz</c>.</param>
internal sealed record CompactReplayTraceManifest(
    string Format, string Layout, string LayoutSha256, int PayloadBytes, int RecordCount, int InitialRecord,
    string MovieSha256, string SourceCheckpointsSha256, string UpdatesSha256, string InitialSha256, string ExpectedSha256);

/// <summary>
/// A converted SMV replay's update manifest and native checkpoints, from either storage form:
/// the private full capture (<c>updates.json</c> and <c>update-boundaries.wram.gz</c>) or the
/// committed compact trace (<c>compact-manifest.json</c>, <c>updates.json.gz</c>,
/// <c>initial.wram.gz</c> and <c>expected.bin.gz</c>; see <see cref="ReplayTraceCompactor"/>).
/// Opening checks every identity the stored form carries before any record is read.
/// </summary>
internal sealed class ReplayTrace : IDisposable
{
    /// <summary>The converter's update manifest in a full capture directory.</summary>
    public const string UpdatesJsonFile = "updates.json";
    /// <summary>The full capture's gzip checkpoint stream.</summary>
    public const string CheckpointStreamFile = "update-boundaries.wram.gz";
    /// <summary>The compact trace's identity manifest.</summary>
    public const string CompactManifestFile = "compact-manifest.json";
    /// <summary>The converter's unchanged update manifest, gzip-compressed.</summary>
    public const string CompactUpdatesFile = "updates.json.gz";
    /// <summary>The initial record, header and complete WRAM, gzip-compressed.</summary>
    public const string CompactInitialFile = "initial.wram.gz";
    /// <summary>Every record's header and delta-coded layout payload, gzip-compressed.</summary>
    public const string CompactExpectedFile = "expected.bin.gz";
    /// <summary>The compact trace format this reader accepts.</summary>
    public const string CompactFormat = "super-metroid-compact-replay-trace-v1";

    /// <summary>Property naming shared by the manifest writer and reader.</summary>
    internal static readonly JsonSerializerOptions ManifestJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

    /// <summary>Holds a trace's update map, source digest and checkpoint reader.</summary>
    private ReplayTrace(JsonElement updates, string sourceCheckpointsSha256, NativeCheckpointRecords records)
    {
        Updates = updates;
        SourceCheckpointsSha256 = sourceCheckpointsSha256;
        Records = records;
    }

    /// <summary>The converter's update manifest (<c>updates.json</c>) root.</summary>
    public JsonElement Updates { get; }

    /// <summary>SHA-256 of the full capture stream these checkpoints come from.</summary>
    public string SourceCheckpointsSha256 { get; }

    /// <summary>The checkpoint records, read forward only.</summary>
    public NativeCheckpointRecords Records { get; }

    /// <summary>Opens a private full capture after checking its stream against the update manifest.</summary>
    /// <param name="directory">Directory holding <c>updates.json</c> and <c>update-boundaries.wram.gz</c>.</param>
    public static ReplayTrace OpenFullCapture(string directory)
    {
        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException($"Missing full native capture directory {Path.GetFullPath(directory)}.");
        JsonElement root;
        using (FileStream updates = File.OpenRead(Path.Combine(directory, UpdatesJsonFile)))
            root = ParseJson(updates);
        FileStream file = File.OpenRead(Path.Combine(directory, CheckpointStreamFile));
        string checkpointHash = Convert.ToHexString(SHA256.HashData(file));
        if (checkpointHash != root.GetProperty("checkpointsSha256").GetString())
        {
            file.Dispose();
            throw new InvalidDataException("Native checkpoint stream does not match the converted manifest.");
        }
        file.Position = 0;
        return new ReplayTrace(root, checkpointHash, new FullCheckpointRecords(file));
    }

    /// <summary>
    /// Opens a committed compact trace after checking its format, its layout against
    /// <paramref name="layout"/>, every compact file's hash, and that its update manifest names the
    /// same movie, source capture and initial record as the compact manifest.
    /// </summary>
    /// <param name="directory">The compact trace directory.</param>
    /// <param name="layout">The layout the calling verifier reads.</param>
    public static ReplayTrace OpenCompact(string directory, ReplayTraceLayout layout)
    {
        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException(
                $"Missing compact replay trace directory {Path.GetFullPath(directory)}. Produce it from the movie's full native " +
                "capture with: SuperMetroid.Verification --compact-replay-trace SOURCE_DIR DEST_DIR.");
        CompactReplayTraceManifest manifest = ReadCompactManifest(directory);
        if (manifest.Format != CompactFormat)
            throw new InvalidDataException($"Unsupported compact replay trace format {manifest.Format}.");
        if (manifest.Layout != layout.Name || manifest.LayoutSha256 != layout.Sha256 || manifest.PayloadBytes != layout.PayloadByteCount)
            throw new InvalidDataException(
                $"Compact replay trace {directory} was packed with layout {manifest.Layout} ({manifest.LayoutSha256}); this verifier " +
                $"reads {layout.Name} ({layout.Sha256}). Recompact it from its full native capture.");
        if (manifest.InitialRecord < 0 || manifest.InitialRecord >= manifest.RecordCount)
            throw new InvalidDataException($"Compact replay trace initial record {manifest.InitialRecord} is outside its {manifest.RecordCount} records.");
        RequireHash(directory, CompactUpdatesFile, manifest.UpdatesSha256);
        RequireHash(directory, CompactInitialFile, manifest.InitialSha256);

        JsonElement root;
        using (FileStream updates = File.OpenRead(Path.Combine(directory, CompactUpdatesFile)))
        using (var decompressed = new GZipStream(updates, CompressionMode.Decompress))
            root = ParseJson(decompressed);
        if (root.GetProperty("checkpointsSha256").GetString() != manifest.SourceCheckpointsSha256)
            throw new InvalidDataException("Compact replay trace checkpoints come from a different native capture than its converted manifest names.");
        if (root.GetProperty("movieSha256").GetString() != manifest.MovieSha256)
            throw new InvalidDataException("Compact replay trace manifest names a different movie than its converted manifest.");
        if (root.GetProperty("initialRecord").GetInt32() != manifest.InitialRecord)
            throw new InvalidDataException("Compact replay trace initial record disagrees with its converted manifest.");

        NativeCheckpoint initial = ReadInitialRecord(Path.Combine(directory, CompactInitialFile));
        FileStream file = File.OpenRead(Path.Combine(directory, CompactExpectedFile));
        if (Convert.ToHexString(SHA256.HashData(file)) != manifest.ExpectedSha256)
        {
            file.Dispose();
            throw new InvalidDataException($"{CompactExpectedFile} in {directory} does not match its compact manifest.");
        }
        file.Position = 0;
        return new ReplayTrace(root, manifest.SourceCheckpointsSha256,
            new CompactCheckpointRecords(file, layout, (manifest.RecordCount, manifest.InitialRecord), initial));
    }

    /// <summary>Reads and deserializes a compact trace's identity manifest.</summary>
    /// <param name="directory">The compact trace directory.</param>
    public static CompactReplayTraceManifest ReadCompactManifest(string directory) =>
        JsonSerializer.Deserialize<CompactReplayTraceManifest>(File.ReadAllText(Path.Combine(directory, CompactManifestFile)), ManifestJson)
            ?? throw new InvalidDataException($"Empty {CompactManifestFile} in {directory}.");

    /// <summary>SHA-256 of a file, as upper-case hexadecimal.</summary>
    /// <param name="path">The file to hash.</param>
    public static string HashFile(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    /// <inheritdoc />
    public void Dispose() => Records.Dispose();

    /// <summary>Requires a trace file to match its manifest digest.</summary>
    private static void RequireHash(string directory, string name, string expected)
    {
        if (HashFile(Path.Combine(directory, name)) != expected)
            throw new InvalidDataException($"{name} in {directory} does not match its compact manifest.");
    }

    /// <summary>Reads the single complete initial record of a compact trace.</summary>
    private static NativeCheckpoint ReadInitialRecord(string path)
    {
        using FileStream file = File.OpenRead(path);
        using var stream = new GZipStream(file, CompressionMode.Decompress);
        var header = new byte[NativeCheckpointRecords.HeaderBytes];
        var workRam = new byte[ReplayTraceMemoryCatalog.WorkRamByteCount];
        stream.ReadExactly(header);
        stream.ReadExactly(workRam);
        if (stream.ReadByte() != -1)
            throw new InvalidDataException($"{CompactInitialFile} holds more than one checkpoint record.");
        return new NativeCheckpoint(BinaryPrimitives.ReadInt32LittleEndian(header),
            BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(sizeof(int))), NativeCheckpointMemory.FromWorkRam(workRam));
    }

    /// <summary>Parses a JSON document and returns a detached copy of its root.</summary>
    private static JsonElement ParseJson(Stream stream)
    {
        using JsonDocument document = JsonDocument.Parse(stream);
        return document.RootElement.Clone();
    }
}
