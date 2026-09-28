namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Read access to the console's mutable WRAM and battery-backed SRAM. Unlike
/// the legacy CPU-bus reader, this contract cannot return cartridge bytes.
/// </summary>
public interface ISnesMutableMemory
{
    /// <summary>Reads a byte from physical WRAM bank $7E or $7F.</summary>
    byte ReadWorkRamByte(int cpuAddress);

    /// <summary>Reads a byte from a bank-$70-$7D/$F0-$FF SRAM window.</summary>
    byte ReadSaveRamByte(int cpuAddress);
}
