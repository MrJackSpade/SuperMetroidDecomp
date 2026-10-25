using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Hardware;

/// <summary>An address space a test writes into: a test bus, a guard over one, or a seeded fixture.</summary>
[CpuBusAccess]
internal interface ISnesWritableAddressSpace : ISnesAddressSpace
{
    /// <summary>
    /// Writes a byte to a mutable mapped region. Implementations must reject ROM and
    /// unimplemented hardware writes rather than silently discarding them.
    /// </summary>
    void WriteByte(int address, byte value);
}
