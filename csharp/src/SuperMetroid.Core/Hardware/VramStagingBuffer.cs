namespace SuperMetroid.Core.Hardware;

/// <summary>
/// A game-owned byte buffer that stands where the cartridge keeps a WRAM staging surface. Queued VRAM transfers read it
/// when the queue drains, exactly as the cartridge's NMI DMA reads WRAM at that point.
/// </summary>
public sealed class VramStagingBuffer
{
    /// <summary>The buffer's bytes in native address order.</summary>
    private readonly byte[] _bytes;

    /// <summary>Allocates a zeroed buffer of <paramref name="byteCount"/> bytes.</summary>
    public VramStagingBuffer(int byteCount) => _bytes = new byte[Ensure.AtLeastZero(byteCount)];

    /// <summary>The number of bytes in the buffer.</summary>
    public int Length => _bytes.Length;

    /// <summary>The buffer's current bytes, as a drained transfer reads them.</summary>
    public ReadOnlySpan<byte> Bytes => _bytes;

    /// <summary>A byte view of <paramref name="length"/> bytes starting <paramref name="start"/> bytes in.</summary>
    internal StagingByteView ByteView(int start, int length) => new(_bytes, start, length);

    /// <summary>A little-endian word view of <paramref name="count"/> words starting <paramref name="start"/> bytes in.</summary>
    internal StagingWordView WordView(int start, int count) => new(_bytes, start, count);
}
