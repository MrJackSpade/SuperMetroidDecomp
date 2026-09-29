namespace SuperMetroid.Core.Hardware;

/// <summary>Little-endian reads from live WRAM, with native fixed-bank offset wrapping.</summary>
public static class SnesWorkRam
{
    /// <summary>Reads two WRAM bytes; the typed source rejects cartridge, SRAM, and peripheral windows.</summary>
    public static ushort ReadWord(ISnesMutableMemory memory, int address)
    {
        ArgumentNullException.ThrowIfNull(memory);
        int highAddress = (address & 0xff0000) | ((address + 1) & 0xffff);
        return (ushort)(memory.ReadWorkRamByte(address) | memory.ReadWorkRamByte(highAddress) << 8);
    }
}
