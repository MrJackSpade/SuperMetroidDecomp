using System.Buffers.Binary;
using System.IO.Compression;
using System.Text.Json;
namespace SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Bosses.Ridley;

/// <summary>
/// Converts a private full native capture (<c>updates.json</c> plus <c>update-boundaries.wram.gz</c>,
/// whose records each hold the complete 128 KiB WRAM) into the committed compact trace that the
/// replay verifiers read (<see cref="ReplayTrace.OpenCompact"/>):
/// <list type="bullet">
/// <item><c>updates.json.gz</c>: the converter's manifest, byte for byte, gzip-compressed;</item>
/// <item><c>initial.wram.gz</c>: the record at the manifest's <c>initialRecord</c>, header and complete
/// WRAM, for the one initial-state import;</item>
/// <item><c>expected.bin.gz</c>: every record in capture order, its header followed by the layout's
/// payload XORed with the previous record's payload (see <see cref="CompactCheckpointRecords"/>);</item>
/// <item><c>compact-manifest.json</c>: format, layout name and hash, record counts, the source movie
/// and capture hashes and each compact file's hash.</item>
/// </list>
/// After writing, the trace is reopened through the verifier-side reader and every record's header
/// and layout bytes are compared with the full capture.
/// </summary>
internal static class ReplayTraceCompactor
{
    /// <summary>Compacts a full capture into a new or empty directory, choosing the layout from the movie identity.</summary>
    /// <param name="sourceTraceDirectory">The full native capture directory.</param>
    /// <param name="compactTraceDirectory">The compact trace directory to create; must be absent or empty.</param>
    [VerificationCommand("--compact-replay-trace", "sourceTraceDirectory", "compactTraceDirectory",
        Description = "Compacts a full native SMV capture into the committed replay-trace form and verifies the round trip.")]
    internal static int CompactReplayTraceCommand(string sourceTraceDirectory, string compactTraceDirectory)
    {
        CompactReplayTraceManifest manifest = Compact(sourceTraceDirectory, compactTraceDirectory, LayoutForMovie);
        long bytes = Directory.EnumerateFiles(compactTraceDirectory).Sum(path => new FileInfo(path).Length);
        Console.WriteLine($"Compacted {manifest.RecordCount} checkpoints with layout {manifest.Layout} " +
            $"({manifest.PayloadBytes} WRAM bytes per record) into {compactTraceDirectory}: {bytes} bytes; round trip verified.");
        return 0;
    }

    /// <summary>The layout of the verifier that replays the movie with SHA-256 <paramref name="movieSha256"/>.</summary>
    /// <param name="movieSha256">The movie identity from <c>updates.json</c>.</param>
    internal static ReplayTraceLayout LayoutForMovie(string movieSha256)
    {
        if (movieSha256 == RidleyFullMovieVerification.RidleyMovieSha256)
            return ReplayTraceMemoryCatalog.RidleyShowcase;
        string[] playthroughs =
        [
            FullMovieReplayVerification.FullPlaythroughMovieSha256,
            FullMovieReplayVerification.LowPercentPlaythroughMovieSha256,
            FullMovieReplayVerification.DrunkPlaythroughMovieSha256,
            FullMovieReplayVerification.MotherBrainGlitchMovieSha256,
            FullMovieReplayVerification.LowcPlaythroughMovieSha256,
        ];
        return playthroughs.Contains(movieSha256) ? ReplayTraceMemoryCatalog.MovieDesync
            : throw new InvalidDataException($"No replay verifier replays movie {movieSha256}; its compact layout is unknown.");
    }

    /// <summary>Writes the compact trace, then verifies it record by record against the full capture.</summary>
    /// <param name="sourceDirectory">The full native capture directory.</param>
    /// <param name="destinationDirectory">The compact trace directory to create; must be absent or empty.</param>
    /// <param name="layoutForMovie">Selects the layout from the movie's SHA-256.</param>
    internal static CompactReplayTraceManifest Compact(string sourceDirectory, string destinationDirectory,
        Func<string, ReplayTraceLayout> layoutForMovie)
    {
        if (Directory.Exists(destinationDirectory) && Directory.EnumerateFileSystemEntries(destinationDirectory).Any())
            throw new IOException($"Compact trace destination {Path.GetFullPath(destinationDirectory)} is not empty; compaction never overwrites.");
        string movieSha256, sourceCheckpointsSha256;
        int initialRecord;
        // Opening the full capture checks its stream against the converted manifest's checkpoint hash.
        using (ReplayTrace source = ReplayTrace.OpenFullCapture(sourceDirectory))
        {
            movieSha256 = (source.Updates.GetProperty("movieSha256").GetString() ?? throw new InvalidOperationException("Verification requires source.Updates.GetProperty(\"movieSha256\").GetString()."));
            initialRecord = source.Updates.GetProperty("initialRecord").GetInt32();
            sourceCheckpointsSha256 = source.SourceCheckpointsSha256;
        }
        ReplayTraceLayout layout = layoutForMovie((movieSha256 ?? throw new InvalidOperationException("Verification requires movieSha256.")));
        Directory.CreateDirectory(destinationDirectory);

        using (FileStream input = File.OpenRead(Path.Combine(sourceDirectory, ReplayTrace.UpdatesJsonFile)))
        using (var output = new GZipStream(File.Create(Path.Combine(destinationDirectory, ReplayTrace.CompactUpdatesFile)), CompressionLevel.SmallestSize))
            input.CopyTo(output);

        int recordCount = WriteRecords(sourceDirectory, destinationDirectory, layout, initialRecord);
        if (initialRecord >= recordCount)
            throw new InvalidDataException($"The full capture has {recordCount} records; its initial record {initialRecord} is missing.");

        var manifest = new CompactReplayTraceManifest(
            ReplayTrace.CompactFormat, layout.Name, layout.Sha256, layout.PayloadByteCount, recordCount, initialRecord,
            movieSha256, sourceCheckpointsSha256,
            ReplayTrace.HashFile(Path.Combine(destinationDirectory, ReplayTrace.CompactUpdatesFile)),
            ReplayTrace.HashFile(Path.Combine(destinationDirectory, ReplayTrace.CompactInitialFile)),
            ReplayTrace.HashFile(Path.Combine(destinationDirectory, ReplayTrace.CompactExpectedFile)));
        File.WriteAllText(Path.Combine(destinationDirectory, ReplayTrace.CompactManifestFile),
            JsonSerializer.Serialize(manifest, ReplayTrace.ManifestJson) + "\n");
        VerifyRoundTrip(sourceDirectory, destinationDirectory, layout);
        return manifest;
    }

    /// <summary>
    /// Confirms that <paramref name="compactDirectory"/>, read through the verifier-side reader, holds
    /// the full capture's update manifest and, for every record, its header and every layout byte.
    /// </summary>
    /// <param name="sourceDirectory">The full native capture directory.</param>
    /// <param name="compactDirectory">The compact trace directory.</param>
    /// <param name="layout">The layout the compact trace must have been packed with.</param>
    internal static void VerifyRoundTrip(string sourceDirectory, string compactDirectory, ReplayTraceLayout layout)
    {
        using ReplayTrace compact = ReplayTrace.OpenCompact(compactDirectory, layout);
        using (FileStream original = File.OpenRead(Path.Combine(sourceDirectory, ReplayTrace.UpdatesJsonFile)))
        using (FileStream packedFile = File.OpenRead(Path.Combine(compactDirectory, ReplayTrace.CompactUpdatesFile)))
        using (var packed = new GZipStream(packedFile, CompressionMode.Decompress))
            RequireSameStream(original, packed);

        int initialRecord = compact.Updates.GetProperty("initialRecord").GetInt32();
        using FileStream sourceFile = File.OpenRead(Path.Combine(sourceDirectory, ReplayTrace.CheckpointStreamFile));
        using var source = new GZipStream(sourceFile, CompressionMode.Decompress);
        var record = new byte[NativeCheckpointRecords.HeaderBytes + ReplayTraceMemoryCatalog.WorkRamByteCount];
        ReadOnlySpan<byte> workRam = record.AsSpan(NativeCheckpointRecords.HeaderBytes);
        for (int index = 0; TryReadRecord(source, record); index++)
        {
            NativeCheckpoint checkpoint = compact.Records.Read(index);
            int frame = BinaryPrimitives.ReadInt32LittleEndian(record), pc = BinaryPrimitives.ReadInt32LittleEndian(record.AsSpan(sizeof(int)));
            if (checkpoint.SourceFrame != frame || checkpoint.BoundaryPc != pc)
                throw new InvalidDataException($"Compact record {index} header differs from the full capture.");
            if (index == initialRecord && (!checkpoint.Memory.IsFullWorkRam ||
                !checkpoint.Memory.Slice(0, ReplayTraceMemoryCatalog.WorkRamByteCount).SequenceEqual(workRam)))
                throw new InvalidDataException($"Compact initial record {index} is not the full capture's complete WRAM.");
            foreach (WorkRamRange range in layout.Ranges)
            {
                if (!checkpoint.Memory.Slice(range.Start, range.Length).SequenceEqual(workRam.Slice(range.Start, range.Length)))
                    throw new InvalidDataException($"Compact record {index} differs from the full capture in WRAM ${range.Start:X5}+{range.Length}.");
            }
        }
        if (!compact.Records.IsExhausted())
            throw new InvalidDataException("The compact trace holds records the full capture does not.");
    }

    /// <summary>Writes each full-capture record's layout payload XORed with the previous one; returns the record count.</summary>
    private static int WriteRecords(string sourceDirectory, string destinationDirectory, ReplayTraceLayout layout, int initialRecord)
    {
        using FileStream sourceFile = File.OpenRead(Path.Combine(sourceDirectory, ReplayTrace.CheckpointStreamFile));
        using var source = new GZipStream(sourceFile, CompressionMode.Decompress);
        using var expected = new GZipStream(File.Create(Path.Combine(destinationDirectory, ReplayTrace.CompactExpectedFile)), CompressionLevel.SmallestSize);
        var record = new byte[NativeCheckpointRecords.HeaderBytes + ReplayTraceMemoryCatalog.WorkRamByteCount];
        var payload = new byte[layout.PayloadByteCount];
        var previous = new byte[layout.PayloadByteCount];
        var delta = new byte[layout.PayloadByteCount];
        int count = 0;
        for (; TryReadRecord(source, record); count++)
        {
            layout.Pack(record.AsSpan(NativeCheckpointRecords.HeaderBytes), payload);
            for (int offset = 0; offset < delta.Length; offset++)
                delta[offset] = (byte)(payload[offset] ^ previous[offset]);
            expected.Write(record, 0, NativeCheckpointRecords.HeaderBytes);
            expected.Write(delta);
            (previous, payload) = (payload, previous);
            if (count == initialRecord)
            {
                using var initial = new GZipStream(File.Create(Path.Combine(destinationDirectory, ReplayTrace.CompactInitialFile)), CompressionLevel.SmallestSize);
                initial.Write(record);
            }
        }
        return count;
    }

    /// <summary>Reads one whole record; false at a clean end of stream, and throws on a truncated record.</summary>
    private static bool TryReadRecord(Stream stream, byte[] record)
    {
        int read = stream.ReadAtLeast(record, record.Length, throwOnEndOfStream: false);
        if (read == 0)
            return false;
        return read == record.Length ? true : throw new InvalidDataException("The full native capture ends inside a checkpoint record.");
    }

    /// <summary>Requires two streams to hold identical bytes.</summary>
    private static void RequireSameStream(Stream expected, Stream actual)
    {
        var left = new byte[1 << 16];
        var right = new byte[left.Length];
        while (true)
        {
            int count = expected.ReadAtLeast(left, left.Length, throwOnEndOfStream: false);
            int wanted = Math.Max(count, 1);
            int actualCount = actual.ReadAtLeast(right.AsSpan(0, wanted), wanted, throwOnEndOfStream: false);
            if (actualCount != count || !left.AsSpan(0, count).SequenceEqual(right.AsSpan(0, count)))
                throw new InvalidDataException($"{ReplayTrace.CompactUpdatesFile} is not the full capture's {ReplayTrace.UpdatesJsonFile}.");
            if (count == 0)
                return;
        }
    }
}
