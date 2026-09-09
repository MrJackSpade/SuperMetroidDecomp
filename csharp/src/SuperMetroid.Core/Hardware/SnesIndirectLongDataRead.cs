namespace SuperMetroid.Core.Hardware;

/// <summary>Data-read portion of a 16-bit LDA [dp],Y for this cartridge's LoROM map.</summary>
/// <remarks>
/// The final indirect-pointer fetch drives its bank byte onto the CPU data bus. An
/// unconnected expansion read retains it; the second data byte sees the first byte's
/// value. This models a particular instruction's bus context, not arbitrary high-level
/// reads as if they were a CPU instruction stream. Untranslated hardware still throws.
/// </remarks>
public static class SnesIndirectLongDataRead
{
    public static ushort ReadWord(ISnesAddressSpace bus, byte pointerBank, ushort pointer, ushort y)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int address = ((pointerBank << 16) + pointer + y) & 0xffffff;
        byte low = ReadDataByte(bus, address, pointerBank);
        byte high = ReadDataByte(bus, (address + 1) & 0xffffff, low);
        return (ushort)(low | high << 8);
    }

    private static byte ReadDataByte(ISnesAddressSpace bus, int address, byte busLatch)
    {
        int bank = address >> 16;
        int offset = address & 0xffff;
        if ((bank & LoRomExpansionReadMap.MirrorBankMask) < LoRomExpansionReadMap.SystemBankLimit &&
            offset >= LoRomExpansionReadMap.ExpansionStart && offset < LoRomExpansionReadMap.RomStart)
            return busLatch;
        return bus.ReadByte(address);
    }
}
