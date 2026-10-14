using System.IO.Compression;

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
