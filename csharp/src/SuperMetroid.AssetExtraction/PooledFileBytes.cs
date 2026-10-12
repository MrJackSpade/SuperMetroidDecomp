using System.Buffers;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// One installed file read into a pooled buffer for a single hash check and parse. Asset
/// compilers copy everything they keep, so the bytes return to the pool on dispose.
/// </summary>
internal sealed class PooledFileBytes : IDisposable
{
    private byte[]? buffer;
    private readonly int length;

    private PooledFileBytes(byte[] buffer, int length)
    {
        this.buffer = buffer;
        this.length = length;
    }

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

    public ReadOnlySpan<byte> Span => Buffer.AsSpan(0, length);

    /// <summary>A read-only view whose buffer a JSON reader may parse in place.</summary>
    public MemoryStream OpenRead() => new(Buffer, 0, length, writable: false, publiclyVisible: true);

    private byte[] Buffer => buffer ?? throw new ObjectDisposedException(nameof(PooledFileBytes));

    public void Dispose()
    {
        if (buffer is null) return;
        ArrayPool<byte>.Shared.Return(buffer);
        buffer = null;
    }
}
