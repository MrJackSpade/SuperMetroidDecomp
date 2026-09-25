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
        foreach ((ushort first, ushort last) in new[]
        {
            (ChozoStatuePlmProgramDefinitions.CrumblePlugStart,
                ChozoStatuePlmProgramDefinitions.CrumblePlugEnd),
            (ChozoStatuePlmProgramDefinitions.LowerNorfairHandStart,
                ChozoStatuePlmProgramDefinitions.LowerNorfairHandEnd),
            (ChozoStatuePlmProgramDefinitions.ClearSlopeStart,
                ChozoStatuePlmProgramDefinitions.ClearSlopeEnd),
            (ChozoStatuePlmProgramDefinitions.BlockSlopeStart,
                ChozoStatuePlmProgramDefinitions.BlockSlopeEnd),
        })
        {
            for (int address = first; address <= last; address++)
            {
                AssertTrue(ChozoStatuePlmProgramDefinitions.TryReadMechanicsByte(
                    checked((ushort)address), out byte compiled),
                    $"Chozo program claims byte $84:{address:X4}");
                AssertEqual(rom.ReadByte(0x840000 | address), compiled,
                    $"Chozo program byte $84:{address:X4} matches cartridge");
                if (address == last) continue;
                AssertTrue(ChozoStatuePlmProgramDefinitions.TryReadMechanicsWord(
                    checked((ushort)address), out ushort compiledWord),
                    $"Chozo program claims word $84:{address:X4}");
                AssertEqual(ReadChozoStatuePlmWord(rom, 0x840000 | address),
                    compiledWord,
                    $"Chozo program word $84:{address:X4} matches cartridge");
            }
            AssertTrue(!ChozoStatuePlmProgramDefinitions.TryReadMechanicsWord(
                last, out _),
                $"Chozo program word cannot cross out of list ending $84:{last:X4}");
            AssertTrue(!ChozoStatuePlmProgramDefinitions.TryReadMechanicsByte(
                checked((ushort)(last + 1)), out _),
                $"Chozo program does not claim adjacent native code after $84:{last:X4}");
        }
        AssertTrue(RoomPlmSharedDeleteProgramDefinitions.TryReadMechanicsWord(
            RoomPlmSharedDeleteProgramDefinitions.Start, out ushort sharedDelete),
            "shared delete list is compiled");
        AssertEqual(ReadChozoStatuePlmWord(rom,
                0x840000 | RoomPlmSharedDeleteProgramDefinitions.Start),
            sharedDelete, "shared delete list matches cartridge");
        AssertTrue(!RoomPlmSharedDeleteProgramDefinitions.TryReadMechanicsWord(
            RoomPlmSharedDeleteProgramDefinitions.End, out _),
            "shared delete list refuses a word crossing its boundary");
        VerifyChozoStatueVisualInstallation(rom);
        Console.WriteLine(
            "Chozo statue PLMs: five headers, four bounded instruction streams, shared delete, native draws and visual-only terrain overrides pass.");
    }

    private static ushort ReadChozoStatuePlmWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
