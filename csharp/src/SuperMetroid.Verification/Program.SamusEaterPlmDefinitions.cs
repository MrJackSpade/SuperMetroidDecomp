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
        VerifySamusEaterProgramControls(rom);
        VerifySamusEaterProgramDraws(rom);
        VerifySamusEaterProgramSound(rom);

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

    private static void VerifySamusEaterProgramControls(SuperMetroidAddressSpace rom)
    {
        int[] drawWords = [0xacc1,0xacc5,0xacc9,0xacd2,0xacd6,0xacda,0xacde,0xace4,0xacf0,0xacf4,
            0xad01,0xad05,0xad09,0xad12,0xad16,0xad1a,0xad1e,0xad24,0xad30,0xad34];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool byteOwned = raw is >= 0xacb8 and < 0xad38;
            bool wordOwned = raw is >= 0xacb8 and < 0xad37;
            AssertEqual(byteOwned, SamusEaterPlmProgramDefinitions.TryReadMechanicsByte(address, out byte actualByte), "plant complete byte ownership");
            AssertEqual(wordOwned, SamusEaterPlmProgramDefinitions.TryReadMechanicsWord(address, out ushort actualWord), "plant complete word ownership");
            if (!byteOwned) AssertEqual((byte)0, actualByte, "plant missing byte zero");
            else if (raw != 0xaccd && raw != 0xad0d && !drawWords.Any(pointer => raw == pointer || raw == pointer + 1))
                AssertEqual(rom.ReadByte(0x840000 | raw), actualByte, "plant native control byte and timer operand");
            if (!wordOwned) AssertEqual((ushort)0, actualWord, "plant missing word zero");
            else
            {
                // Every byte-aligned view, including ACF7 crossing between the two lists.
                AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | raw), actualWord, "plant native packed word view");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "plant shared reader ownership");
                AssertEqual(actualWord, shared, "plant shared reader preserves word view");
            }
        }
    }

    private static void VerifySamusEaterProgramDraws(SuperMetroidAddressSpace rom)
    {
        int[] pointers = [0xacc1,0xacc5,0xacc9,0xacd2,0xacd6,0xacda,0xacde,0xace4,0xacf0,0xacf4,
            0xad01,0xad05,0xad09,0xad12,0xad16,0xad1a,0xad1e,0xad24,0xad30,0xad34];
        foreach (int pointer in pointers)
        {
            AssertTrue(SamusEaterPlmProgramDefinitions.TryReadMechanicsWord((ushort)pointer, out ushort actual), "plant draw operand word");
            AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | pointer), actual, "plant original draw selector");
            for (int half = 0; half < 2; half++)
            {
                AssertTrue(SamusEaterPlmProgramDefinitions.TryReadMechanicsByte((ushort)(pointer + half), out byte value), "plant draw operand byte");
                AssertEqual(rom.ReadByte(0x840000 | (pointer + half)), value, "plant original draw operand byte");
            }
        }
    }

    private static void VerifySamusEaterProgramSound(SuperMetroidAddressSpace rom)
    {
        foreach (ushort pointer in new ushort[] {0xaccd,0xad0d})
        {
            AssertTrue(SamusEaterPlmProgramDefinitions.TryReadMechanicsByte(pointer, out byte value), "plant packed sound byte");
            AssertEqual(rom.ReadByte(0x840000 | pointer), value, "plant original sound selection");
        }
    }

    private static ushort ReadSamusEaterPlmWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
