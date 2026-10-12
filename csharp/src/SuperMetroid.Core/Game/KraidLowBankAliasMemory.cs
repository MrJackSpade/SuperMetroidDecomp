using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The live memory Kraid's private bank-$A7 pointers reach when they fall below $8000 (#629).
/// LoROM maps the lower half of bank $A7 to the low-WRAM mirror and the unconnected I/O
/// windows, not to cartridge data, so these exceptional cartridge paths read mutable state.
/// </summary>
[CpuBusBoundary("domain view: Kraid's bank-$A7 pointers below $8000 that alias live memory")]
internal readonly struct KraidLowBankAliasMemory(ISnesAddressSpace bus)
{
    /// <summary>The head-instruction byte at low-half <paramref name="pointer"/>, which must reach the WRAM mirror.</summary>
    internal byte HeadInstructionByte(ushort pointer)
    {
        // The typed reader rejects the unmapped expansion range instead of accepting a
        // synthetic value from an overly permissive fake bus.
        ISnesMutableMemory memory = bus as ISnesMutableMemory ??
            throw new InvalidOperationException("Kraid's live low-half head alias requires WRAM access.");
        return memory.ReadWorkRamByte(KraidBackgroundRomData.NativeBank | pointer);
    }

    /// <summary>The mouth-hitbox byte at low-half <paramref name="pointer"/>: live WRAM, SRAM or a modeled peripheral.</summary>
    internal byte MouthHitboxByte(ushort pointer)
    {
        int address = KraidBackgroundRomData.NativeBank | pointer;
        return SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)) switch
        {
            SnesDmaSourceKind.WorkRam => (bus as ISnesMutableMemory ??
                throw new InvalidOperationException("Kraid mouth alias requires WRAM."))
                .ReadWorkRamByte(address),
            SnesDmaSourceKind.SaveRam => (bus as ISnesMutableMemory ??
                throw new InvalidOperationException("Kraid mouth alias requires SRAM."))
                .ReadSaveRamByte(address),
            SnesDmaSourceKind.Unmapped => (bus as ISnesCpuPeripheralSource ??
                throw new InvalidOperationException(
                    $"CPU read ${SnesAddressMath.BankNumber(address):X2}:{(ushort)address:X4} is outside the runtime address map."))
                .ReadPeripheralByte(address),
            _ => throw new InvalidDataException(
                $"Kraid mouth alias ${address:X6} requires a compiled cartridge definition."),
        };
    }

    /// <summary>
    /// Word <paramref name="word"/> of the head tilemap a low-half <paramref name="sourcePointer"/>
    /// selects: low byte first, the high byte wrapping within the bank.
    /// </summary>
    internal ushort HeadTilemapWord(ushort sourcePointer, int word)
    {
        ISnesMutableMemory memory = bus as ISnesMutableMemory ??
            throw new InvalidOperationException("Enemy staging requires live WRAM.");
        int address = (KraidBackgroundRomData.NativeBank | sourcePointer) + word * 2;
        return memory.ReadWorkRamWordFixedBank(address);
    }
}
