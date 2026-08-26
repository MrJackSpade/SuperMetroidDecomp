namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Minimal byte-wide view of the SNES 24-bit CPU bus used by translated logic and DMA.
/// </summary>
/// <remarks>
/// This boundary is intentionally one byte wide. ROM, WRAM, SRAM, and memory-mapped
/// regions do not share a single convenient backing array on real hardware. A later full
/// runtime can route each bank through its proper device while the VRAM queue remains
/// ignorant of where its source bytes live.
/// </remarks>
public interface ISnesAddressSpace
{
    /// <summary>
    /// Reads a byte from a bank:offset CPU address in the inclusive range
    /// <c>$00:0000-$FF:FFFF</c>.
    /// </summary>
    byte ReadByte(int address);

    /// <summary>
    /// Writes a byte to a mutable mapped region. Implementations must reject ROM and
    /// unimplemented hardware writes rather than silently discarding them.
    /// </summary>
    void WriteByte(int address, byte value);
}
