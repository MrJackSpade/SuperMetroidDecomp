using System.Buffers;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// One installed file read into a pooled buffer for a single hash check and parse. Asset
/// compilers copy everything they keep, so the bytes return to the pool on dispose.
/// </summary>
internal sealed class PooledFileBytes : IDisposable
{
    /// <summary>The rented storage, cleared after it is returned to the shared pool.</summary>
    private byte[]? buffer;
    /// <summary>Number of file bytes populated at the start of the rented storage.</summary>
    private readonly int length;

    /// <summary>Wraps one populated rented buffer for scoped parsing.</summary>
    /// <param name="buffer">Array rented from the shared byte pool.</param>
    /// <param name="length">Number of meaningful bytes in <paramref name="buffer"/>.</param>
    private PooledFileBytes(byte[] buffer, int length)
    {
        this.buffer = buffer;
        this.length = length;
    }

    /// <summary>Reads an entire installed file into storage rented from the shared byte pool.</summary>
    /// <param name="path">File whose bytes are needed for one hash-and-parse operation.</param>
    /// <returns>An owner that returns its rented storage when disposed.</returns>
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

    /// <summary>Gets the populated portion of the rented buffer for hashing.</summary>
    public ReadOnlySpan<byte> Span => Buffer.AsSpan(0, length);

    /// <summary>A read-only view whose buffer a JSON reader may parse in place.</summary>
    public MemoryStream OpenRead() => new(Buffer, 0, length, writable: false, publiclyVisible: true);

    /// <summary>Gets the live rented buffer or rejects access after disposal.</summary>
    private byte[] Buffer => buffer ?? throw new ObjectDisposedException(nameof(PooledFileBytes));

    /// <summary>Returns the rented byte storage exactly once and invalidates further views.</summary>
    public void Dispose()
    {
        if (buffer is null) return;
        ArrayPool<byte>.Shared.Return(buffer);
        buffer = null;
    }
}
