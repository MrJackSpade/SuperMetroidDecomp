using System.IO.Compression;

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
            if (frame != initial.SourceFrame || pc != initial.BoundaryPc || !packedInitial.SequenceEqual(payload))
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
