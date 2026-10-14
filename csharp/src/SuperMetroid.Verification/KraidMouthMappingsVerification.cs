using SuperMetroid.Core.Hardware;
using static KraidMouthHitboxesVerification;
using SuperMetroid.Core.Bosses.Kraid;

/// <summary>Checks Kraid mouth hitbox mappings and reads around the bank-$A7 low-half boundary.</summary>
internal static class KraidMouthMappingsVerification
{
    /// <summary>Compares all eight authored mouth shapes and rejects non-shape pointers between them.</summary>
    internal static void VerifyKraidMouthShapeCases(SuperMetroidAddressSpace rom)
    {
        short Word(int address) => unchecked((short)(SnesLittleEndian.Word(KraidMouthShapeCasesRomByte(rom, address), KraidMouthShapeCasesRomByte(rom, address + 1))));
        for (int entry = 0; entry < 8; entry++)
        {
            ushort pointer = (ushort)(0x9788 + 8 * entry);
            int address = 0xa70000 | pointer;
            var expected = (Word(address), Word(address + 2), Word(address + 4), Word(address + 6));
            AssertTrue(KraidMouthHitboxDefinitions.IsDefined(pointer), "Native mouth shape is defined");
            AssertEqual(expected, KraidMouthHitboxDefinitions.Resolve(pointer), "Native mouth shape case");
            AssertEqual((expected.Item1, expected.Item2, expected.Item4),
                KraidMouthHitboxDefinitions.ResolveCollision(rom, pointer), "Collision uses left/top/bottom only");
        }
        for (int raw = 0x9787; raw <= 0x97c8; raw++)
        {
            ushort pointer = (ushort)raw;
            bool expected = raw >= 0x9788 && raw <= 0x97c0 && (raw - 0x9788) % 8 == 0;
            AssertEqual(expected, KraidMouthHitboxDefinitions.IsDefined(pointer), "Native shape alignment");
            if (!expected)
                AssertThrows<InvalidDataException>(() => KraidMouthHitboxDefinitions.Resolve(pointer), "Non-shape address rejects");
        }
    }

    /// <summary>Reads an original shape-table byte for the authored-case comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid mouth shape cases")]
    private static byte KraidMouthShapeCasesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks compatibility bytes and word reads that cross the $7FFF/$8000 pointer boundary.</summary>
    internal static void VerifyKraidLowHalfBoundaryMapping(SuperMetroidAddressSpace rom)
    {
        AssertEqual(7, KraidMouthHitboxDefinitions.LowHalfBoundaryLength, "Native compatibility window length");
        for (int index = 0; index < 7; index++)
            AssertEqual(KraidLowHalfBoundaryMappingRomByte(rom, 0xa78000 + index), KraidMouthHitboxDefinitions.LowHalfBoundaryByte(index),
                "Native instruction byte in low-half compatibility window");
        AssertThrows<IndexOutOfRangeException>(() => KraidMouthHitboxDefinitions.LowHalfBoundaryByte(-1), "Negative boundary byte");
        AssertThrows<IndexOutOfRangeException>(() => KraidMouthHitboxDefinitions.LowHalfBoundaryByte(7), "Past boundary byte");
        var bus = new KraidMouthBoundaryReadBus();
        byte ExpectedByte(ushort pointer) => pointer < 0x8000
            ? KraidMouthBoundaryReadBus.Value(pointer) : KraidLowHalfBoundaryMappingRomByte(rom, 0xa70000 | pointer);
        short ExpectedWord(ushort pointer) => unchecked((short)(SnesLittleEndian.Word(ExpectedByte(pointer), ExpectedByte((ushort)(pointer + 1)))));
        for (ushort pointer = 0x7ff9; pointer <= 0x7fff; pointer++)
            AssertEqual((ExpectedWord(pointer), ExpectedWord((ushort)(pointer + 2)), ExpectedWord((ushort)(pointer + 6))),
                KraidMouthHitboxDefinitions.ResolveCollision(bus, pointer), "Mouth boundary crossing uses original bytes");
        for (ushort pointer = 0x7ffd; pointer <= 0x7fff; pointer++)
            AssertEqual(unchecked((ushort)ExpectedWord((ushort)(pointer + 2))),
                KraidHeadInstructionDefinitions.ReadGrowthSelectionWord(bus, pointer), "Head boundary crossing uses original bytes");
    }

    /// <summary>Reads a bank-$A7 compatibility byte from the original cartridge.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid low half boundary mapping")]
    private static byte KraidLowHalfBoundaryMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
