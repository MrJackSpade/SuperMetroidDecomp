namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The native Mode 7 transfer queue in WRAM <c>$02D0-$032F</c>, for one main-loop frame. The port applies the
/// transfers to VRAM itself, but the queued descriptor bytes stay in WRAM after the NMI handler
/// (<c>HandleMode7Transfers</c>, $80:8BBA) clears only the first control word, and running-cadence stage five
/// later reads them as its delay list at <c>$91:0303</c> (#1338). This view writes exactly the bytes
/// <c>QueueMode7Transfers</c> leaves behind.
/// </summary>
[CpuBusBoundary("domain view: the native Mode 7 transfer queue, whose residue running cadence stage five reads")]
internal struct Mode7TransferQueue(ISnesAddressSpace bus)
{
    /// <summary><c>Mode7Stack</c> ($0334): the byte offset of the next free descriptor in this frame's queue.</summary>
    private int _stack;

    /// <summary>
    /// Queues one VRAM tilemap descriptor as $80:8B8B does: nine bytes copied in 16-bit words, the final
    /// <c>AND #$00FF</c> increment word also clearing the following byte.
    /// </summary>
    internal void QueueVramTilemapTransfer(SnesAddress source, ushort size, ushort destination)
    {
        if (_stack + Mode7TransferQueueLayout.VramDescriptorByteCount + 1 > Mode7TransferQueueLayout.QueueByteCount)
            throw new InvalidOperationException($"Mode 7 transfer queue offset {_stack} has no room for another descriptor.");
        Write(0, Mode7TransferQueueLayout.VramTilemapControl);
        Write(1, SnesLittleEndian.LowByte(source.Offset));
        Write(2, SnesLittleEndian.HighByte(source.Offset));
        Write(3, source.Bank);
        Write(4, SnesLittleEndian.LowByte(size));
        Write(5, SnesLittleEndian.HighByte(size));
        Write(6, SnesLittleEndian.LowByte(destination));
        Write(7, SnesLittleEndian.HighByte(destination));
        Write(8, 0);
        Write(9, 0);
        _stack += Mode7TransferQueueLayout.VramDescriptorByteCount;
    }

    /// <summary>The NMI handler's completion: <c>STZ Mode7Transfer.control</c> (a word) and <c>STZ Mode7Stack</c>.</summary>
    internal void CompleteTransfers()
    {
        if (_stack == 0)
            return;
        bus.WriteByte(Mode7TransferQueueLayout.QueueStart, 0);
        bus.WriteByte(Mode7TransferQueueLayout.QueueStart + 1, 0);
        _stack = 0;
    }

    /// <summary>
    /// The queue byte at WRAM <paramref name="offset"/>, which bank $91's low-half delay-list reads alias: running cadence
    /// stage five's <c>$91:0303</c>, or the pose-table overread of invalid poses at <c>$91:0302</c>. Only the queue's
    /// bytes are a modeled cell; any other low-half offset is corrupt state.
    /// </summary>
    internal static byte ReadResidueByte(ISnesAddressSpace bus, ushort offset)
    {
        int address = (int)new SnesAddress(SnesLittleEndian.BankByte(Mode7TransferQueueLayout.QueueStart), offset);
        if (address is < Mode7TransferQueueLayout.QueueStart or
            >= Mode7TransferQueueLayout.QueueStart + Mode7TransferQueueLayout.QueueByteCount)
            throw new InvalidDataException($"Bank $91 low-half byte ${offset:X4} lies outside the Mode 7 transfer queue.");
        return (bus as ISnesMutableMemory ?? throw new InvalidOperationException(
            "The Mode 7 transfer queue's residue requires live WRAM.")).ReadWorkRamByte(address);
    }

    /// <summary>Writes one byte at <paramref name="offset"/> past the current descriptor.</summary>
    private readonly void Write(int offset, byte value) =>
        bus.WriteByte(Mode7TransferQueueLayout.QueueStart + _stack + offset, value);
}
