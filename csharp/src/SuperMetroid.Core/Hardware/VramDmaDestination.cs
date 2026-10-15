namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Where a queued VRAM DMA lands (#630): a VMADD word and the VMAIN increment. Super Metroid's
/// queue stores both in one word, bit 15 selecting the 32-word column increment; that encoding
/// stays inside this type, the queue and the PPU backend.
/// </summary>
[PpuLayoutBoundary("codec: queued VRAM DMA destination word")]
public readonly record struct VramDmaDestination
{
    /// <summary>Native DMA queue bit 15, selecting 32-word column increments.</summary>
    private const ushort ColumnFlag = 1 << 15;
    /// <summary>Native fifteen-bit VMADD address field.</summary>
    private const ushort WordMask = SnesPpuLayout.VramWordCount - 1;
    /// <summary>The words a column increment advances.</summary>
    private const int ColumnIncrementWords = 32;

    /// <summary>Preserves an already encoded native DMA destination word.</summary>
    private VramDmaDestination(ushort encoded) => Encoded = encoded;

    /// <summary>The native queue word: VMADD in bits 0-14, bit 15 set for the column increment.</summary>
    [PpuLayoutAccess]
    public ushort Encoded { get; }

    /// <summary>The destination <paramref name="wordOffset"/> words into the VRAM described by <paramref name="cell"/>.</summary>
    [PpuLayoutAccess]
    public static VramDmaDestination At(PpuLayoutCell cell, int wordOffset = 0, VramIncrement increment = VramIncrement.Row)
    {
        Ensure.NotNull(cell);
        if (cell.Region != PpuLayoutRegion.Vram)
            throw new ArgumentException($"{cell} is not a VRAM region.", nameof(cell));
        if ((uint)wordOffset >= (uint)cell.Count)
            throw new ArgumentOutOfRangeException(nameof(wordOffset), wordOffset, $"{cell.Name} holds {cell.Count} words.");
        return At(cell.Offset + wordOffset, increment);
    }

    /// <summary>The VMADD word the transfer starts at.</summary>
    internal int Word => Encoded & WordMask;

    /// <summary>The words VMADD advances after each high-byte write: one for a row, 32 for a column.</summary>
    internal int WordIncrement => (Encoded & ColumnFlag) == 0 ? 1 : ColumnIncrementWords;

    /// <summary>Decodes a native queue word read from cartridge or WRAM data.</summary>
    [PpuLayoutAccess]
    public static VramDmaDestination FromNative(ushort encoded) => new(encoded);

    /// <summary>The destination at physical VMADD <paramref name="word"/>.</summary>
    internal static VramDmaDestination At(int word, VramIncrement increment) =>
        (uint)word <= WordMask
            ? new((ushort)(word | (increment == VramIncrement.Column ? ColumnFlag : 0)))
            : throw new ArgumentOutOfRangeException(nameof(word), word, "VMADD holds 15 bits.");
}
