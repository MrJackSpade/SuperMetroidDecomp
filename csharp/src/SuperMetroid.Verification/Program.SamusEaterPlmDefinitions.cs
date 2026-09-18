using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifySamusEaterPlmDefinitions(SuperMetroidAddressSpace rom)
    {
        ReadOnlySpan<SamusEaterPlmDefinition> definitions = SamusEaterPlmDefinitions.All;
        AssertEqual(2, definitions.Length, "Samus Eater PLM definition count");

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
            "Samus Eater PLMs: both native header/list identities and real aligned spawns pass without runtime header reads.");
    }

    private static ushort ReadSamusEaterPlmWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
