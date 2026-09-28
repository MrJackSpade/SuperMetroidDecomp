namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Write boundary for mutable SNES state used by translated logic.
/// </summary>
/// <remarks>
/// ROM, WRAM, SRAM, and peripheral reads have separate contracts. No generic read is
/// exposed here: a caller must declare which source owns the byte it needs.
/// </remarks>
public interface ISnesAddressSpace
{
    /// <summary>
    /// Writes a byte to a mutable mapped region. Implementations must reject ROM and
    /// unimplemented hardware writes rather than silently discarding them.
    /// </summary>
    void WriteByte(int address, byte value);
}
