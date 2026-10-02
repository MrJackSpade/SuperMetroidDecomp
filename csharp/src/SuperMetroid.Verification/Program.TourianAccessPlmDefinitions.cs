using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyTourianAccessPlmDefinitions(SuperMetroidAddressSpace rom)
    {
        VerifyTourianAccessHeaderSelection(rom);
        VerifyTourianAccessInstructionSelection(rom);
        VerifyDefinition(clear: false);
        VerifyDefinition(clear: true);

        Console.WriteLine(
            "Tourian access PLMs: both native header/list identities and real highest-slot spawns pass without runtime header reads.");

        void VerifyDefinition(bool clear)
        {
            TourianAccessPlmDefinition definition = TourianAccessPlmDefinitions.ForState(clear);
            const int width = 16;
            const int height = 16;
            int blockCount = width * height;
            var level = new RoomLevelData(
                width,
                height,
                new ushort[blockCount],
                new byte[blockCount],
                new ushort[blockCount],
                new byte[8]);
            var plms = new RoomPlmSystem();

            AssertTrue(
                plms.TrySpawnTourianAccess(level, clear),
                $"Tourian access {(clear ? "clear" : "crumble")} actor allocates");
            AssertEqual(1, plms.ActiveCount, "Tourian access spawn occupies one PLM slot");
            RoomPlmSlotSnapshot slot = plms.PopulationSlots.Single();
            AssertEqual(39, slot.NativeSlotIndex, "Tourian access uses the highest free native slot");
            AssertEqual(definition.HeaderPointer, slot.HeaderPointer, "Tourian access header identity");
            AssertEqual(12 * width + 6, slot.BlockIndex, "Tourian access fixed block coordinate");
            AssertEqual(
                definition.InstructionListPointer,
                slot.InstructionPointer,
                "Tourian access compiled initial instruction list");
            AssertEqual(1, slot.InstructionTimer, "Tourian access starts on timer one");
        }
    }

    private static void VerifyTourianAccessHeaderSelection(SuperMetroidAddressSpace rom) =>
        VerifyTourianAccessSpawnField(rom, false);

    private static void VerifyTourianAccessInstructionSelection(SuperMetroidAddressSpace rom) =>
        VerifyTourianAccessSpawnField(rom, true);

    private static void VerifyTourianAccessSpawnField(SuperMetroidAddressSpace rom, bool instruction)
    {
        // Native spawn operands: completed descent crumbles; already-unlocked room clears.
        (bool Clear, int SpawnOperand)[] cases = [(false,0x88dc9f), (true,0x88db99)];
        foreach (var item in cases)
        {
            ushort header = ReadTourianAccessWord(rom, item.SpawnOperand);
            var actual = TourianAccessPlmDefinitions.ForState(item.Clear);
            AssertEqual(instruction ? ReadTourianAccessWord(rom, 0x840000 | (header + 2)) : header,
                instruction ? actual.InstructionListPointer : actual.HeaderPointer,
                $"Tourian native spawn selection clear={item.Clear} instruction={instruction}");
            AssertEqual((byte)6, rom.ReadByte(item.SpawnOperand - 2), "Tourian native spawn column");
            AssertEqual((byte)12, rom.ReadByte(item.SpawnOperand - 1), "Tourian native spawn row");
        }
    }

    private static ushort ReadTourianAccessWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
