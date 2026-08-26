namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Debuggable representation of Super Metroid's ordinary VRAM write table at WRAM
/// <c>$00D0-$02CF</c>, consumed during NMI by routine <c>$80:8C83</c>.
/// </summary>
/// <remarks>
/// The ROM stores tightly packed seven-byte records:
/// <code>
/// +0  u16 transfer byte count (zero terminates the table)
/// +2  u16 source offset
/// +4  u8  source bank
/// +5  u16 encoded VRAM word destination
/// </code>
/// This port uses typed records but preserves the byte-counted tail because hundreds of
/// original call sites advance that tail by seven. Seeing <see cref="TailInBytes"/> in the
/// debugger therefore remains directly comparable to WRAM <c>$0330</c>.
/// </remarks>
public sealed class VramWriteQueue
{
    /// <summary>Bytes reserved by the original WRAM layout before the Mode 7 table.</summary>
    public const int StorageByteCount = 0x200;

    /// <summary>Physical size of one packed queue entry.</summary>
    public const int EntryByteCount = 7;

    // A two-byte zero-size terminator is written at TailInBytes during NMI. Consequently,
    // an otherwise fitting final record is invalid if it leaves fewer than two bytes.
    private const int TerminatorByteCount = 2;

    private readonly List<VramWriteEntry> _entries = [];

    /// <summary>
    /// Current logical entries in append order. The collection cannot be cast back to a
    /// mutable List, keeping TailInBytes and the record list synchronized.
    /// </summary>
    public IReadOnlyList<VramWriteEntry> Entries => _entries;

    /// <summary>
    /// Byte offset at which the ROM would write the next record, corresponding to WRAM
    /// <c>$0330</c>. It advances by seven rather than by one.
    /// </summary>
    public int TailInBytes { get; private set; }

    /// <summary>
    /// Appends the typed equivalent of one packed seven-byte record.
    /// </summary>
    public void Enqueue(ushort sizeInBytes, int sourceAddress, ushort encodedVramDestination)
    {
        if (sizeInBytes == 0)
            throw new ArgumentOutOfRangeException(nameof(sizeInBytes), "Zero is reserved for the queue terminator.");
        if ((uint)sourceAddress > 0x00ff_ffff)
            throw new ArgumentOutOfRangeException(nameof(sourceAddress), sourceAddress, "Source must be a 24-bit SNES CPU address.");

        int newTail = TailInBytes + EntryByteCount;
        if (newTail + TerminatorByteCount > StorageByteCount)
        {
            throw new InvalidOperationException(
                $"VRAM write queue overflow: entry would place the tail at ${newTail:X3}, " +
                $"leaving no two-byte terminator inside the ${StorageByteCount:X3}-byte table.");
        }

        _entries.Add(new VramWriteEntry(sizeInBytes, sourceAddress, encodedVramDestination));
        TailInBytes = newTail;
    }

    /// <summary>
    /// Executes every queued transfer in insertion order, then clears the table just as
    /// the NMI consumer does at <c>$80:8CC9</c>.
    /// </summary>
    public void DrainTo(SnesVram vram, ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(bus);

        // Use an index rather than foreach to make append-during-drain behavior explicit:
        // this object is single-threaded like the original main-loop/NMI handshake.
        for (int index = 0; index < _entries.Count; index++)
        {
            VramWriteEntry entry = _entries[index];
            vram.ExecuteQueuedWrite(bus, entry.SourceAddress, entry.SizeInBytes, entry.EncodedVramDestination);
        }

        // The assembly always zeroes the tail, even when the queue was empty. Clearing the
        // typed records at the same point prevents stale transfers on the next frame.
        _entries.Clear();
        TailInBytes = 0;
    }
}

/// <summary>
/// One logical form of the packed record consumed by <c>$80:8C83</c>.
/// </summary>
/// <param name="SizeInBytes">DMA byte count written to <c>DAS1</c>.</param>
/// <param name="SourceAddress">24-bit CPU bus address written to <c>A1T1/A1B1</c>.</param>
/// <param name="EncodedVramDestination">
/// Destination word address; bit 15 selects a 32-word rather than one-word increment.
/// </param>
public readonly record struct VramWriteEntry(
    ushort SizeInBytes,
    int SourceAddress,
    ushort EncodedVramDestination);
