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
    /// <summary>Reads the little-endian data word addressed by an already fetched long pointer plus Y, carrying across banks and wrapping at 24 bits; each byte independently selects memory, a compiled definition, a device, or the proven undriven expansion window.</summary>
    /// <param name="bus">Live WRAM/SRAM source and optional peripheral responder; cartridge bytes must come from the supplied compiled definition, not generic bus reads.</param>
    /// <param name="pointerBank">Fetched pointer bank byte, also the initial data-bus latch for an undriven first read.</param>
    /// <param name="pointer">Fetched pointer's 16-bit offset; this method does not fetch the direct-page pointer itself.</param>
    /// <param name="y">Unsigned 16-bit index added to the full long address without bank-local wrapping.</param>
    /// <param name="cartridgeDefinition">Optional contiguous compiled bytes for cartridge-classified operands; an empty span permits no cartridge reads.</param>
    /// <param name="definitionAddress">Full CPU address corresponding to the first compiled byte; lookup uses exact addresses rather than resolving cartridge mirrors.</param>
    /// <returns>The low byte combined with the following byte; an undriven second read retains the first byte's value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidOperationException">An operand requires unavailable live memory, an unimplemented device, or cartridge bytes outside the compiled definition.</exception>
    public static ushort ReadWord(ISnesAddressSpace bus, byte pointerBank, ushort pointer, ushort y,
        ReadOnlySpan<byte> cartridgeDefinition = default, int definitionAddress = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int address = ((pointerBank << 16) + pointer + y) & SnesCpuAddressLayout.AddressMask;
        byte low = ReadDataByte(bus, address, pointerBank, cartridgeDefinition, definitionAddress);
        byte high = ReadDataByte(bus, (address + 1) & SnesCpuAddressLayout.AddressMask, low, cartridgeDefinition, definitionAddress);
        return (ushort)(low | high << 8);
    }

    private static byte ReadDataByte(ISnesAddressSpace bus, int address, byte busLatch,
        ReadOnlySpan<byte> cartridgeDefinition, int definitionAddress)
    {
        int bank = address >> 16;
        int offset = address & 0xffff;
        if ((bank & LoRomExpansionReadMap.MirrorBankMask) < LoRomExpansionReadMap.SystemBankLimit &&
            offset >= LoRomExpansionReadMap.ExpansionStart && offset < LoRomExpansionReadMap.RomStart)
            return busLatch;
        var kind = SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address));
        if (kind == SnesDmaSourceKind.Cartridge && (uint)(address - definitionAddress) < cartridgeDefinition.Length)
            return cartridgeDefinition[address - definitionAddress];
        return kind switch
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
