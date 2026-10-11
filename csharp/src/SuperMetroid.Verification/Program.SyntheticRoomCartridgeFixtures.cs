using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Seeds a bank-$8F room PLM population list at <paramref name="population"/> in the sparse
    /// cartridge fixture. <paramref name="records"/> are the native six-byte placement records
    /// (header pointer, X, Y, argument) followed by whatever terminator bytes the test supplies.
    /// </summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for a bank-$8F room PLM population list")]
    static void SeedRoomPlmPopulation(TestAddressSpace bus, ushort population, ReadOnlySpan<byte> records) =>
        bus.WriteBytes(0x8f0000 | population, records);

    /// <summary>
    /// Creates a sparse cartridge fixture whose bank $84 (PLM headers, instruction lists and
    /// draw lists, <c>$84:8000-$84:FFFF</c>) holds exactly <paramref name="bank84"/>.
    /// </summary>
    static TestAddressSpace CreateBank84Cartridge(ReadOnlySpan<byte> bank84)
    {
        if (bank84.Length != 0x8000)
            throw new ArgumentException("Bank $84 is exactly $8000 bytes.", nameof(bank84));
        var bus = new TestAddressSpace();
        SeedPlmProgramBytes(bus, 0x8000, bank84);
        return bus;
    }

    /// <summary>
    /// Seeds a one-record population at <paramref name="population"/> placing the elevator
    /// platform PLM header (<c>$84:B70B</c>) at <paramref name="block"/>,
    /// and points that header's instruction-list word (<c>$84:B70D</c>) at <paramref name="instructionList"/>.
    /// </summary>
    static void SeedSoleElevatorPlatformPlm(
        TestAddressSpace bus, ushort population, Position16<RoomBlocks> block, ushort instructionList)
    {
        SeedRoomPlmPopulation(bus, population,
            [0x0b, 0xb7, unchecked((byte)block.X), unchecked((byte)block.Y), 0x00, 0x00, 0x00, 0x00]);
        SeedPlmProgramWords(bus, 0xb70d, instructionList);
    }

    /// <summary>
    /// Seeds consecutive little-endian words of bank-$84 PLM data (headers, instruction
    /// lists, operands or draw records) starting at the bank-local <paramref name="pointer"/>.
    /// Each word's address wraps within bank $84 exactly as the native 16-bit list cursor does.
    /// </summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for bank-$84 PLM header and program words")]
    static void SeedPlmProgramWords(TestAddressSpace bus, ushort pointer, params ReadOnlySpan<ushort> words)
    {
        for (int index = 0; index < words.Length; index++)
            WriteTestWord(bus, 0x840000 | unchecked((ushort)(pointer + index * sizeof(ushort))), words[index]);
    }

    /// <summary>
    /// Seeds raw bank-$84 PLM bytes (instruction streams with unaligned operands, draw
    /// records) starting at the bank-local <paramref name="pointer"/>.
    /// </summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for bank-$84 PLM program bytes")]
    static void SeedPlmProgramBytes(TestAddressSpace bus, ushort pointer, ReadOnlySpan<byte> bytes) =>
        bus.WriteBytes(0x840000 | pointer, bytes);

    /// <summary>
    /// Seeds the retail one-word bank-$84 delete list (<c>PLMEntries_nothing</c>,
    /// <see cref="RoomPlmInstructionLists.Delete"/>) in the sparse cartridge fixture: a single
    /// <see cref="RoomPlmInstruction.Delete"/> instruction.
    /// </summary>
    static void SeedPlmDeleteList(TestAddressSpace bus) =>
        SeedPlmProgramWords(bus, RoomPlmInstructionLists.Delete, (ushort)RoomPlmInstruction.Delete);

    /// <summary>
    /// Seeds an empty bank-$A1 room enemy population (a lone <c>$FFFF</c> terminator) at
    /// <paramref name="population"/> in the sparse cartridge fixture.
    /// </summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for an empty bank-$A1 room enemy population")]
    static void SeedEmptyRoomEnemyPopulation(TestAddressSpace bus, ushort population) =>
        bus.WriteBytes(0xa10000 | population, [0xff, 0xff]);
}
