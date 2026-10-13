using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using static VerificationHelpersVerification;

internal static class ReplayTraceCompactionVerification
{
    /// <summary>Records in the synthetic capture: folded prelude, initial, two updates, a skipped wait record and the terminal.</summary>
    private const int SyntheticRecordCount = 6;

    /// <summary>
    /// Builds a small full-layout capture with a folded prelude, a skipped hardware-wait record and a
    /// boot-entering update without a checkpoint, compacts it, and confirms the replay verifiers'
    /// reader returns the same updates, boundaries, initial WRAM and compared bytes from both forms.
    /// A missing, corrupted, re-laid-out or mismatched compact trace must fail when opened or read.
    /// </summary>
    internal static void VerifyReplayTraceCompaction()
    {
        using var scratch = new TestScratchDirectory("replay-trace-compaction");
        string source = Path.Combine(scratch.Root, "full");
        string compact = Path.Combine(scratch.Root, "compact");
        byte[] movie = "synthetic replay-trace compaction movie"u8.ToArray();
        WriteSyntheticCapture(source, movie);
        ReplayTraceLayout layout = ReplayTraceMemoryCatalog.MovieDesync;

        CompactReplayTraceManifest manifest = ReplayTraceCompactor.Compact(source, compact, _ => layout);
        AssertEqual(SyntheticRecordCount, manifest.RecordCount, "compacted record count");
        AssertEqual(1, manifest.InitialRecord, "compacted initial record");
        AssertEqual(layout.Sha256, manifest.LayoutSha256, "compacted layout identity");
        AssertThrows<IOException>(() => ReplayTraceCompactor.Compact(source, compact, _ => layout),
            "compaction refuses to overwrite an existing trace");

        using (var full = NativeMovieCheckpoints.OpenFullCapture(source, movie))
        using (var packed = NativeMovieCheckpoints.Open(compact, movie))
        {
            AssertSequenceEqual(full.Updates, packed.Updates, "converted updates");
            AssertEqual(full.SourceFrameCount, packed.SourceFrameCount, "source frame count");
            AssertEqual(full.InitialInput, packed.InitialInput, "initial input");
            AssertEqual(full.InitialRecord, packed.InitialRecord, "initial record");
            for (int update = 0; update <= full.Updates.Count; update++)
            {
                NativeCheckpointMemory? expected = full.ReadAfter(update);
                NativeCheckpointMemory? actual = packed.ReadAfter(update);
                AssertEqual(expected is null, actual is null, $"update {update} checkpoint presence");
                if (expected is null || actual is null)
                    continue;
                if (update == 0)
                {
                    AssertTrue(actual.IsFullWorkRam, "the initial checkpoint keeps complete WRAM for the one import");
                    AssertSequenceEqual(expected.ToWorkRam(), actual.ToWorkRam(), "initial WRAM");
                }
                else
                {
                    AssertThrows<InvalidOperationException>(() => actual.Word(ReplayTraceMemoryCatalog.OamStart),
                        $"update {update}: a read outside the layout fails instead of returning a stand-in");
                }
                foreach (WorkRamRange range in layout.Ranges)
                    AssertTrue(expected.Slice(range.Start, range.Length).SequenceEqual(actual.Slice(range.Start, range.Length)),
                        $"update {update} WRAM ${range.Start:X4}+{range.Length} round trip");
                AssertEqual(expected.Word(MovieDesyncMemory.GameState), actual.Word(MovieDesyncMemory.GameState), $"update {update} game state");
                AssertEqual(expected.Word(MovieDesyncMemory.Random), actual.Word(MovieDesyncMemory.Random), $"update {update} RNG");
                AssertEqual(expected.Word(MovieDesyncMemory.EnemyBase + MovieDesyncMemory.EnemyStride * 31 + MovieDesyncMemory.EnemyHealthOffset),
                    actual.Word(MovieDesyncMemory.EnemyBase + MovieDesyncMemory.EnemyStride * 31 + MovieDesyncMemory.EnemyHealthOffset),
                    $"update {update} last enemy slot health");
            }
            full.AssertExhausted();
            packed.AssertExhausted();
        }

        // Identity checks the verifiers perform against the converted manifest still apply.
        AssertThrows<InvalidDataException>(() => NativeMovieCheckpoints.Open(compact, [.. movie, 0]),
            "a compact trace of another movie is rejected");
        var missing = AssertThrows<DirectoryNotFoundException>(() => NativeMovieCheckpoints.Open(Path.Combine(scratch.Root, "absent"), movie),
            "a missing compact trace fails");
        AssertTrue(missing.Message.Contains("Missing compact replay trace directory", StringComparison.Ordinal), "missing trace message names the directory");
        AssertThrows<InvalidDataException>(() => ReplayTrace.OpenCompact(compact, ReplayTraceMemoryCatalog.RidleyShowcase).Dispose(),
            "a trace packed with another layout is rejected");

        RejectCorruption(scratch.Root, compact, movie, "flipped-expected", directory =>
            FlipByte(Path.Combine(directory, ReplayTrace.CompactExpectedFile)), "a corrupted checkpoint stream");
        RejectCorruption(scratch.Root, compact, movie, "flipped-initial", directory =>
            FlipByte(Path.Combine(directory, ReplayTrace.CompactInitialFile)), "a corrupted initial record");
        RejectCorruption(scratch.Root, compact, movie, "layout-hash", directory =>
            RewriteManifest(directory, value => value with { LayoutSha256 = new string('0', value.LayoutSha256.Length) }), "a different layout hash");
        RejectCorruption(scratch.Root, compact, movie, "source-capture", directory =>
            RewriteManifest(directory, value => value with { SourceCheckpointsSha256 = new string('0', value.SourceCheckpointsSha256.Length) }),
            "a different source capture");
        RejectCorruption(scratch.Root, compact, movie, "format", directory =>
            RewriteManifest(directory, value => value with { Format = "super-metroid-compact-replay-trace-v0" }), "an unknown format");

        // A stream rewritten with its manifest hash updated is internally consistent but disagrees
        // with the complete initial record; reading that record fails.
        string inconsistent = CopyTrace(scratch.Root, compact, "inconsistent-initial");
        RewriteExpected(inconsistent, layout, recordIndex: 1);
        using (var checkpoints = NativeMovieCheckpoints.Open(inconsistent, movie))
            AssertThrows<InvalidDataException>(() => checkpoints.ReadAfter(0), "a payload disagreeing with the initial record is rejected");
        Console.WriteLine($"Replay trace compaction: {SyntheticRecordCount} records round-trip through the {layout.Name} layout " +
            $"({layout.PayloadByteCount} bytes per record); corrupted and mismatched traces are rejected.");
    }

    private static void RejectCorruption(string root, string compact, byte[] movie, string name, Action<string> corrupt, string context)
    {
        string directory = CopyTrace(root, compact, name);
        corrupt(directory);
        AssertThrows<InvalidDataException>(() => NativeMovieCheckpoints.Open(directory, movie).Dispose(), context + " is rejected");
    }

    private static string CopyTrace(string root, string compact, string name)
    {
        string directory = Path.Combine(root, name);
        Directory.CreateDirectory(directory);
        foreach (string file in Directory.EnumerateFiles(compact))
            File.Copy(file, Path.Combine(directory, Path.GetFileName(file)));
        return directory;
    }

    private static void FlipByte(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        bytes[bytes.Length / 2] ^= 1;
        File.WriteAllBytes(path, bytes);
    }

    private static void RewriteManifest(string directory, Func<CompactReplayTraceManifest, CompactReplayTraceManifest> change) =>
        File.WriteAllText(Path.Combine(directory, ReplayTrace.CompactManifestFile),
            JsonSerializer.Serialize(change(ReplayTrace.ReadCompactManifest(directory)), ReplayTrace.ManifestJson));

    /// <summary>Changes one payload byte of <paramref name="recordIndex"/> and re-hashes the stream in the manifest.</summary>
    private static void RewriteExpected(string directory, ReplayTraceLayout layout, int recordIndex)
    {
        string path = Path.Combine(directory, ReplayTrace.CompactExpectedFile);
        byte[] records;
        using (var input = new GZipStream(File.OpenRead(path), CompressionMode.Decompress))
        using (var buffer = new MemoryStream())
        {
            input.CopyTo(buffer);
            records = buffer.ToArray();
        }
        int recordBytes = NativeCheckpointRecords.HeaderBytes + layout.PayloadByteCount;
        records[recordIndex * recordBytes + NativeCheckpointRecords.HeaderBytes] ^= 1;
        using (var output = new GZipStream(File.Create(path), CompressionLevel.Fastest))
            output.Write(records);
        RewriteManifest(directory, value => value with { ExpectedSha256 = ReplayTrace.HashFile(path) });
    }

    /// <summary>
    /// Writes <c>updates.json</c> and <c>update-boundaries.wram.gz</c> in the converter's full layout:
    /// record 0 is a folded prelude, record 1 the initial state, update 2 skips record 3 (a removed
    /// hardware wait), update 3 enters the boot clear without a checkpoint, and record 5 is terminal.
    /// </summary>
    private static void WriteSyntheticCapture(string directory, byte[] movie)
    {
        Directory.CreateDirectory(directory);
        int[] recordFrames = [9, 10, 11, 12, 13, 16];
        string stream = Path.Combine(directory, ReplayTrace.CheckpointStreamFile);
        var random = new Random(1266);
        using (var output = new GZipStream(File.Create(stream), CompressionLevel.Fastest))
        {
            var record = new byte[NativeCheckpointRecords.HeaderBytes + ReplayTraceMemoryCatalog.WorkRamByteCount];
            for (int index = 0; index < SyntheticRecordCount; index++)
            {
                BinaryPrimitives.WriteInt32LittleEndian(record, recordFrames[index]);
                BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(sizeof(int)),
                    index == SyntheticRecordCount - 1 ? 0 : MovieDesyncMemory.ControllerReadBoundary);
                // The first record fills all of WRAM; later ones change a few hundred bytes, as gameplay does.
                if (index == 0)
                    random.NextBytes(record.AsSpan(NativeCheckpointRecords.HeaderBytes));
                else
                    for (int change = 0; change < 400; change++)
                        record[NativeCheckpointRecords.HeaderBytes + random.Next(ReplayTraceMemoryCatalog.WorkRamByteCount)] = (byte)random.Next(256);
                output.Write(record);
            }
        }

        JsonObject Update(int number, int sourceFrame, int? expectedRecord) => new()
        {
            ["update"] = number,
            ["sourceFrame"] = sourceFrame,
            ["input"] = number * 16,
            ["kind"] = "main-loop",
            ["timingClass"] = "main-loop",
            ["expectedRecord"] = expectedRecord,
            ["excludedNmiAfter"] = 0,
            ["hardwareWaitLatch"] = null,
            ["timingEvidence"] = new JsonObject { ["doorLoaderCompletedEnemySlots"] = null },
            ["messageBoxStartFrame"] = null,
            ["messageBoxEndFrame"] = null,
        };
        var manifest = new JsonObject
        {
            ["format"] = "super-metroid-gameplay-updates-v7",
            ["initialRecord"] = 1,
            ["movieSha256"] = Convert.ToHexString(SHA256.HashData(movie)),
            ["checkpointsSha256"] = ReplayTrace.HashFile(stream),
            ["sourceFrameCount"] = recordFrames[^1],
            ["initialInput"] = 0,
            ["updateCount"] = 4,
            ["updates"] = new JsonArray(Update(1, 10, 2), Update(2, 11, 4), Update(3, 13, null), Update(4, 14, 5)),
        };
        File.WriteAllText(Path.Combine(directory, ReplayTrace.UpdatesJsonFile), manifest.ToJsonString(ReplayTrace.ManifestJson) + "\n");
    }
}
