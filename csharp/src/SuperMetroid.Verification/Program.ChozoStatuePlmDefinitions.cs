using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyChozoStatuePlmDefinitions(SuperMetroidAddressSpace rom)
    {
        ReadOnlySpan<ChozoStatuePlmDefinition> definitions = ChozoStatuePlmDefinitions.All;
        AssertEqual(5, definitions.Length, "Chozo terrain PLM definition count");

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
        VerifyChozoStatueVisualInstallation(rom);
        Console.WriteLine(
            "Chozo statue PLMs: all five header/list identities, native draw extraction and visual-only terrain overrides pass.");
    }

    private static ushort ReadChozoStatuePlmWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
