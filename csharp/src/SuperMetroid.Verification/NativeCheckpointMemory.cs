using System.Buffers.Binary;

/// <summary>
/// Read-only native WRAM at one replay checkpoint: either the complete 128 KiB image (a full
/// capture, or a compact trace's initial record) or only a <see cref="ReplayTraceLayout"/>'s ranges.
/// Reading a byte the checkpoint does not hold throws instead of returning a stand-in value, so
/// a verifier read missing from <see cref="ReplayTraceMemoryCatalog"/> fails loudly.
/// </summary>
internal sealed class NativeCheckpointMemory
{
    private readonly byte[] bytes;
    private readonly ReplayTraceLayout? layout;

    private NativeCheckpointMemory(byte[] bytes, ReplayTraceLayout? layout)
    {
        this.bytes = bytes;
        this.layout = layout;
    }

    /// <summary>A checkpoint holding the complete WRAM image.</summary>
    /// <param name="workRam">The 128 KiB WRAM image; retained, not copied.</param>
    public static NativeCheckpointMemory FromWorkRam(byte[] workRam) =>
        workRam.Length == ReplayTraceMemoryCatalog.WorkRamByteCount ? new(workRam, null)
            : throw new ArgumentException("A full checkpoint holds exactly 128 KiB of WRAM.", nameof(workRam));

    /// <summary>A checkpoint holding only <paramref name="layout"/>'s ranges.</summary>
    /// <param name="layout">The layout <paramref name="payload"/> was packed with.</param>
    /// <param name="payload">The packed bytes; retained, not copied.</param>
    public static NativeCheckpointMemory FromPayload(ReplayTraceLayout layout, byte[] payload) =>
        payload.Length == layout.PayloadByteCount ? new(payload, layout)
            : throw new ArgumentException($"A {layout.Name} payload holds {layout.PayloadByteCount} bytes.", nameof(payload));

    /// <summary>Whether every WRAM byte is available.</summary>
    public bool IsFullWorkRam => layout is null;

    /// <summary>The WRAM byte at <paramref name="address"/>.</summary>
    /// <param name="address">WRAM offset ($7E:0000 is zero).</param>
    public byte this[int address] => Slice(address, 1)[0];

    /// <summary>The little-endian WRAM word at <paramref name="address"/>.</summary>
    /// <param name="address">WRAM offset of the low byte.</param>
    public ushort Word(int address) => BinaryPrimitives.ReadUInt16LittleEndian(Slice(address, sizeof(ushort)));

    /// <summary>The <paramref name="length"/> WRAM bytes at <paramref name="address"/>.</summary>
    /// <param name="address">WRAM offset of the first byte.</param>
    /// <param name="length">Number of bytes.</param>
    public ReadOnlySpan<byte> Slice(int address, int length)
    {
        if (layout is null)
            return bytes.AsSpan(address, length);
        if (!layout.TryLocate(address, length, out int offset))
            throw new InvalidOperationException(
                $"WRAM ${address:X4} (+{length} bytes) is outside the compact replay trace layout '{layout.Name}'. " +
                "Add the range to ReplayTraceMemoryCatalog and recompact the trace from its full capture.");
        return bytes.AsSpan(offset, length);
    }

    /// <summary>A copy of the complete WRAM image, for the one initial-state import.</summary>
    public byte[] ToWorkRam() => layout is null ? bytes.ToArray()
        : throw new InvalidOperationException($"A compact '{layout.Name}' checkpoint holds only its layout's ranges, not the complete WRAM image.");
}
