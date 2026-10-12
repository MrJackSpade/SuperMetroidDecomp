using System.Buffers;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// One installed file read into a pooled buffer for a single hash check and parse. Asset
/// compilers copy everything they keep, so the bytes return to the pool on dispose.
/// </summary>
internal sealed class PooledFileBytes : IDisposable
{
    /// <summary>Rented backing array returned to the shared pool when this read is disposed.</summary>
    private byte[]? buffer;
    /// <summary>Actual file length; the rented array may have greater capacity.</summary>
    private readonly int length;

    /// <summary>Retains the rented array and exact byte count for hashing and parsing.</summary>
    private PooledFileBytes(byte[] buffer, int length)
    {
        this.buffer = buffer;
        this.length = length;
    }

    /// <summary>Reads the complete installed file into a rented buffer, returning that buffer if reading fails.</summary>
    public static PooledFileBytes Read(string path)
    {
        using var input = File.OpenRead(path);
        int length = checked((int)input.Length);
        byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            input.ReadExactly(buffer.AsSpan(0, length));
        }
        catch
        {
            ArrayPool<byte>.Shared.Return(buffer);
            throw;
        }
        return new PooledFileBytes(buffer, length);
    }

    /// <summary>Limits parser access to the file's bytes rather than the rented array's spare capacity.</summary>
    public ReadOnlySpan<byte> Span => Buffer.AsSpan(0, length);

    /// <summary>A read-only view whose buffer a JSON reader may parse in place.</summary>
    public MemoryStream OpenRead() => new(Buffer, 0, length, writable: false, publiclyVisible: true);

    /// <summary>Rejects access after the buffer has been returned to the pool.</summary>
    private byte[] Buffer => buffer ?? throw new ObjectDisposedException(nameof(PooledFileBytes));

    /// <summary>Returns the rented array once and rejects later accesses through this wrapper.</summary>
    public void Dispose()
    {
        if (buffer is null) return;
        ArrayPool<byte>.Shared.Return(buffer);
        buffer = null;
    }
}
