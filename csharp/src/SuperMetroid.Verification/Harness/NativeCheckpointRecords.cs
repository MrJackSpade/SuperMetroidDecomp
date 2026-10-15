using System.Buffers.Binary;

namespace SuperMetroid.Verification.Harness;

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
