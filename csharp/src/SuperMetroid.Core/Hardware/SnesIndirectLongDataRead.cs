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
        int address = ((pointerBank << 16) + pointer + y) & SnesCpuAddressLayout.AddressMask;
        byte low = ReadDataByte(bus, address, pointerBank);
        byte high = ReadDataByte(bus, (address + 1) & SnesCpuAddressLayout.AddressMask, low);
        return (ushort)(low | high << 8);
    }

    private static byte ReadDataByte(ISnesAddressSpace bus, int address, byte busLatch)
    {
        int bank = address >> 16;
        int offset = address & 0xffff;
        if ((bank & LoRomExpansionReadMap.MirrorBankMask) < LoRomExpansionReadMap.SystemBankLimit &&
            offset >= LoRomExpansionReadMap.ExpansionStart && offset < LoRomExpansionReadMap.RomStart)
            return busLatch;
        return SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)) switch
        {
            SnesDmaSourceKind.WorkRam => (bus as ISnesMutableMemory ??
                throw new InvalidOperationException("Indirect operand requires WRAM."))
                .ReadWorkRamByte(address),
            SnesDmaSourceKind.SaveRam => (bus as ISnesMutableMemory ??
                throw new InvalidOperationException("Indirect operand requires SRAM."))
                .ReadSaveRamByte(address),
            SnesDmaSourceKind.Unmapped => (bus as ISnesCpuPeripheralSource ??
                throw new InvalidOperationException($"Indirect operand ${address:X6} requires an unimplemented peripheral."))
                .ReadPeripheralByte(address),
            _ => throw new InvalidOperationException(
                $"Indirect operand ${address:X6} requires a compiled cartridge definition."),
        };
    }
}
