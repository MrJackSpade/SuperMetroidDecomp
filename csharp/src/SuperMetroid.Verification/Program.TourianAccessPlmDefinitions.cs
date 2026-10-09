using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks native clear/crumble PLM identities, selected header and instruction pointers, and the resulting room spawn slot.</summary>
    /// <param name="rom">Cartridge address space containing the native Tourian access spawn records.</param>
    private static void VerifyTourianAccessPlmDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyTourianAccessHeaderSelection), () => VerifyTourianAccessHeaderSelection(rom));
        Suite(nameof(VerifyTourianAccessInstructionSelection), () => VerifyTourianAccessInstructionSelection(rom));
        Suite(nameof(VerifyDefinition), () => VerifyDefinition(clear: false));
        Suite(nameof(VerifyDefinition), () => VerifyDefinition(clear: true));

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

    /// <summary>Compares compiled access-PLM header selection with the two native clear-state spawn records.</summary>
    /// <param name="rom">Cartridge address space supplying the native spawn operands.</param>
    private static void VerifyTourianAccessHeaderSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyTourianAccessSpawnField), () => VerifyTourianAccessSpawnField(rom, false));

    /// <summary>Compares compiled access-PLM instruction-list selection with the pointers stored in the native headers.</summary>
    /// <param name="rom">Cartridge address space supplying the native spawn and header words.</param>
    private static void VerifyTourianAccessInstructionSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyTourianAccessSpawnField), () => VerifyTourianAccessSpawnField(rom, true));

    /// <summary>Checks the selected native spawn operand, its optional header instruction pointer, and fixed spawn coordinates.</summary>
    /// <param name="rom">Cartridge address space containing the native spawn records and PLM headers.</param>
    /// <param name="instruction">When <see langword="true"/>, compare the header's instruction-list pointer; otherwise compare the header pointer.</param>
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

    /// <summary>Reads a little-endian 16-bit word from the cartridge address space.</summary>
    /// <param name="bus">Address space containing the word.</param>
    /// <param name="address">Full SNES address of the first byte.</param>
    /// <returns>The two bytes combined with the lower-address byte as the low byte.</returns>
    private static ushort ReadTourianAccessWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
