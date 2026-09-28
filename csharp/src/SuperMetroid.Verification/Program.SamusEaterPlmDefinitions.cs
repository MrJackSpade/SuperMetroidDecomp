using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifySamusEaterPlmDefinitions(SuperMetroidAddressSpace rom)
    {
        ReadOnlySpan<SamusEaterPlmDefinition> definitions = SamusEaterPlmDefinitions.All;
        AssertEqual(2, definitions.Length, "Samus Eater PLM definition count");
        AssertEqual(definitions[0].InstructionListPointer,
            SamusEaterPlmProgramDefinitions.FloorStart,
            "floor plant header enters its compiled instruction program");
        AssertEqual(definitions[1].InstructionListPointer,
            SamusEaterPlmProgramDefinitions.CeilingStart,
            "ceiling plant header enters its compiled instruction program");
        AssertEqual(0xad38, SamusEaterPlmProgramDefinitions.EndExclusive,
            "plant program ends before the Wrecked Ship treadmill");
        for (int address = SamusEaterPlmProgramDefinitions.FloorStart;
             address < SamusEaterPlmProgramDefinitions.EndExclusive; address++)
        {
            AssertTrue(SamusEaterPlmProgramDefinitions.TryReadMechanicsByte(
                    (ushort)address, out byte compiledByte),
                $"Samus Eater program byte $84:{address:X4} is compiled");
            AssertEqual(rom.ReadByte(0x840000 | address), compiledByte,
                $"Samus Eater program byte $84:{address:X4} matches cartridge");
            if (address + 1 == SamusEaterPlmProgramDefinitions.EndExclusive)
                continue;
            AssertTrue(SamusEaterPlmProgramDefinitions.TryReadMechanicsWord(
                    (ushort)address, out ushort compiledWord),
                $"Samus Eater program word $84:{address:X4} is compiled");
            AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | address), compiledWord,
                $"Samus Eater program word $84:{address:X4} matches cartridge");
        }
        AssertTrue(!SamusEaterPlmProgramDefinitions.TryReadMechanicsByte(
                0xacb7, out _) &&
            !SamusEaterPlmProgramDefinitions.TryReadMechanicsByte(0xad38, out _) &&
            !SamusEaterPlmProgramDefinitions.TryReadMechanicsWord(0xad37, out _),
            "Samus Eater program excludes adjacent bank-$84 code");

        foreach (SamusEaterPlmDefinition definition in definitions)
        {
            ushort expected = ReadSamusEaterPlmWord(
                rom,
                0x840000 | unchecked((ushort)(definition.HeaderPointer + 2)));
            AssertEqual(
                expected,
                definition.InstructionListPointer,
                $"Samus Eater PLM ${definition.HeaderPointer:X4} initial list matches cartridge");

            const int width = 4;
            const int height = 4;
            int blockCount = width * height;
            var levelWords = new ushort[blockCount];
            levelWords[5] = 0xb123;
            var level = new RoomLevelData(
                width,
                height,
                levelWords,
                new byte[blockCount],
                new ushort[blockCount],
                new byte[8]);
            var plms = new RoomPlmSystem();
            var samus = new SamusState();
            samus.XPosition = 24;
            samus.YPosition = definition.Ceiling ? (ushort)16 : (ushort)0;
            samus.Kinematics.XRadius = 8;
            samus.Kinematics.YRadius = 16;

            RoomCollisionBlock block = level.GetCollisionBlock(1, 1);
            plms.TrySpawnSamusEater(level, block, definition.HeaderPointer, samus);

            RoomPlmSlotSnapshot slot = plms.PopulationSlots.Single();
            AssertEqual(39, slot.NativeSlotIndex, "Samus Eater uses highest free native slot");
            AssertEqual(definition.HeaderPointer, slot.HeaderPointer, "Samus Eater header identity");
            AssertEqual(5, slot.BlockIndex, "Samus Eater trigger block");
            AssertEqual(
                definition.InstructionListPointer,
                slot.InstructionPointer,
                "Samus Eater compiled initial instruction list");
            AssertEqual(1, slot.InstructionTimer, "Samus Eater starts on timer one");
            AssertEqual(
                0x8123,
                level.GetCollisionBlock(1, 1).LevelWord,
                "Samus Eater deactivates its trigger");
        }

        AssertThrows<InvalidDataException>(
            () => SamusEaterPlmDefinitions.Resolve(0xb6d3),
            "map-station header cannot enter Samus Eater domain");
        Console.WriteLine(
            "Samus Eater PLMs: both header/list identities, 128 compiled control bytes, and real aligned spawns match cartridge.");
    }

    private static ushort ReadSamusEaterPlmWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
