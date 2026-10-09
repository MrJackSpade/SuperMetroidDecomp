using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Verifies Chozo statue terrain headers, instruction programs, callbacks, collision, and installed visuals against cartridge data.</summary>
    private static void VerifyChozoStatuePlmDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyChozoTerrainHeaders), () => VerifyChozoTerrainHeaders(rom));
        Suite(nameof(VerifyChozoProgramControls), () => VerifyChozoProgramControls(rom));
        Suite(nameof(VerifyChozoProgramDraws), () => VerifyChozoProgramDraws(rom));
        Suite(nameof(VerifyChozoProgramTargets), () => VerifyChozoProgramTargets(rom));
        Suite(nameof(VerifyChozoProgramCallback), () => VerifyChozoProgramCallback(rom));
        Suite(nameof(VerifyChozoProgramEvent), () => VerifyChozoProgramEvent(rom));
        AssertTrue(RoomPlmSharedDeleteProgramDefinitions.TryReadMechanicsWord(
            RoomPlmSharedDeleteProgramDefinitions.Start, out ushort sharedDelete),
            "shared delete list is compiled");
        AssertEqual(ReadChozoStatuePlmWord(rom,
                0x840000 | RoomPlmSharedDeleteProgramDefinitions.Start),
            sharedDelete, "shared delete list matches cartridge");
        AssertTrue(!RoomPlmSharedDeleteProgramDefinitions.TryReadMechanicsWord(
            RoomPlmSharedDeleteProgramDefinitions.End, out _),
            "shared delete list refuses a word crossing its boundary");
        Suite(nameof(VerifyChozoLayoutGeometry), () => VerifyChozoLayoutGeometry(rom));
        Suite(nameof(VerifyChozoLayoutCollision), () => VerifyChozoLayoutCollision(rom));
        Suite(nameof(VerifyChozoLayoutVisuals), () => VerifyChozoLayoutVisuals(rom));
        Suite(nameof(VerifyChozoStatueVisualInstallation), () => VerifyChozoStatueVisualInstallation(rom));
        Console.WriteLine(
            "Chozo statue PLMs: five headers, four bounded instruction streams, shared delete, native draws and visual-only terrain overrides pass.");
    }

    /// <summary>Checks the five supported terrain headers, their spawn setup, and Wrecked Ship hand collision.</summary>
    /// <param name="rom">Cartridge address space supplying the original header words.</param>
    private static void VerifyChozoTerrainHeaders(SuperMetroidAddressSpace rom)
    {
        ChozoStatuePlmDefinition[] definitions = ChozoStatuePlmDefinitions.All.ToArray();
        AssertEqual(5, definitions.Length, "Chozo terrain PLM definition count");

        ushort[] headers = [0xd6d6,0xd6ee,0xd6f8,0xd6fc,0xd113];
        AssertTrue(headers.SequenceEqual(definitions.Select(d => d.HeaderPointer)), "Chozo original header enumeration");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort header = (ushort)raw;
            if (!headers.Contains(header))
                AssertThrows<InvalidDataException>(() => ChozoStatuePlmDefinitions.Resolve(header), "Chozo full unsupported header domain");
            else
            {
                var selected = ChozoStatuePlmDefinitions.Resolve(header);
                AssertEqual(header, selected.HeaderPointer, "Chozo dispatch header identity");
                AssertEqual(ReadChozoStatuePlmWord(rom, 0x840000 | (header + 2)), selected.InstructionListPointer,
                    "Chozo dispatch original initial list");
                AssertEqual(definitions.Single(d => d.HeaderPointer == header), selected, "Chozo named/enumerated dispatch agreement");
            }
        }
        foreach (ChozoStatuePlmDefinition definition in definitions)
        {
            ushort expected = ReadChozoStatuePlmWord(
                rom,
                0x840000 | unchecked((ushort)(definition.HeaderPointer + 2)));
            AssertEqual(
                expected,
                definition.InstructionListPointer,
                $"Chozo PLM ${definition.HeaderPointer:X4} initial list matches cartridge");

            const int width = 32;
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
            var request = new ChozoStatuePlmRequest(
                definition.HeaderPointer,
                BlockX: 3,
                BlockY: 4,
                IsHardcoded: false);

            AssertTrue(
                plms.TrySpawnChozoStatuePlm(level, request),
                $"Chozo PLM ${definition.HeaderPointer:X4} allocates");
            RoomPlmSlotSnapshot slot = plms.PopulationSlots.Single();
            AssertEqual(39, slot.NativeSlotIndex, "Chozo PLM uses highest free native slot");
            AssertEqual(definition.HeaderPointer, slot.HeaderPointer, "Chozo PLM header identity");
            AssertEqual(4 * width + 3, slot.BlockIndex, "Chozo PLM requested block coordinate");
            AssertEqual(
                definition.InstructionListPointer,
                slot.InstructionPointer,
                "Chozo PLM compiled initial instruction list");
            AssertEqual(1, slot.InstructionTimer, "Chozo PLM starts on timer one");

            if (definition.HeaderPointer == ChozoStatuePlmRomData.WreckedShipHand)
            {
                RoomCollisionBlock block = level.GetCollisionBlock(3, 4);
                AssertEqual(
                    RoomCollisionType.SpecialBlock,
                    block.CollisionType,
                    "Wrecked Ship hand installs special-block collision");
                AssertEqual(
                    ChozoStatuePlmRomData.WreckedShipHandBts.Value,
                    block.Behavior,
                    "Wrecked Ship hand installs native BTS");
            }
        }

        AssertThrows<InvalidDataException>(
            () => ChozoStatuePlmDefinitions.Resolve(0xd6f2),
            "Chozo collision-trigger header cannot enter terrain-spawn domain");
    }

    /// <summary>Reads a little-endian word from two adjacent cartridge bytes.</summary>
    /// <param name="bus">Cartridge address space to read.</param>
    /// <param name="address">Address of the word's low byte.</param>
    /// <returns>The 16-bit word beginning at <paramref name="address"/>.</returns>
    private static ushort ReadChozoStatuePlmWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
