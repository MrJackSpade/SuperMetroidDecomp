using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace SuperMetroid.Verification.Harness;

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
