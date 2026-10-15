using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>Checks the floor and ceiling Samus Eater PLM headers, instruction programs, and live trigger spawns.</summary>
internal static class SamusEaterPlmDefinitionsVerification
{
    /// <summary>Compares native header and program data and checks aligned floor and ceiling plant activation.</summary>
    internal static void VerifySamusEaterPlmDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifySamusEaterHeaderInstructionMapping), () => VerifySamusEaterHeaderInstructionMapping(rom));
        Suite(nameof(VerifySamusEaterMountingMapping), () => VerifySamusEaterMountingMapping(rom));
        SamusEaterPlmDefinition[] definitions =
            [SamusEaterPlmDefinitions.Resolve(PlmHeaderId.InsideReactionBrinstarFloorPlant), SamusEaterPlmDefinitions.Resolve(PlmHeaderId.InsideReactionBrinstarCeilingPlant)];
        AssertEqual(2, definitions.Length, "Samus Eater PLM definition count");
        AssertEqual(SamusEaterProgram.FloorBite, definitions[0].Program, "floor plant header starts the floor bite");
        AssertEqual(SamusEaterProgram.CeilingBite, definitions[1].Program, "ceiling plant header starts the ceiling bite");
        Suite(nameof(RoomPlmProgramsVerification.VerifySamusEaterPrograms), () => RoomPlmProgramsVerification.VerifySamusEaterPrograms(rom));

        foreach (PlmHeaderId headerPointer in new ushort[] { 0xb6cb, 0xb6cf })
        {
            SamusEaterPlmDefinition definition = SamusEaterPlmDefinitions.Resolve(headerPointer);
            const int width = 4;
            const int height = 4;
            int blockCount = width * height;
            var levelWords = new ushort[blockCount];
            levelWords[5] = 0xb123;
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(levelWords, new byte[blockCount], new ushort[blockCount]), new byte[8]);
            var plms = new RoomPlmSystem();
            var samus = new SamusState();
            samus.Position = samus.Position.WithWhole(new(24, definition.Ceiling ? (ushort)16 : (ushort)0));
            samus.Kinematics.Radius = new(8, 16);

            RoomCollisionBlock block = level.GetCollisionBlock(new(1, 1));
            plms.SamusEater.TrySpawnSamusEater(level, block, headerPointer, samus);

            RoomPlmSlotSnapshot slot = plms.PopulationSlots.Single();
            AssertEqual(39, slot.NativeSlotIndex, "Samus Eater uses highest free native slot");
            AssertEqual(headerPointer, slot.HeaderPointer, "Samus Eater header identity");
            AssertEqual(5, slot.BlockIndex, "Samus Eater trigger block");
            AssertTrue(slot.Animation is SamusEaterPlmAnimation animation && animation.Program == definition.Program,
                "Samus Eater starts its typed bite");
            AssertEqual(1, slot.InstructionTimer, "Samus Eater starts on timer one");
            AssertEqual(
                0x8123,
                level.GetCollisionBlock(new(1, 1)).LevelWord,
                "Samus Eater deactivates its trigger");
        }

        AssertThrows<InvalidDataException>(
            () => SamusEaterPlmDefinitions.Resolve(PlmHeaderId.MapStation),
            "map-station header cannot enter Samus Eater domain");
        Console.WriteLine(
            "Samus Eater PLMs: both header/program identities, the typed programs, and real aligned spawns match cartridge.");
    }

    /// <summary>Checks both plant headers' native initial instruction pointers.</summary>
    private static void VerifySamusEaterHeaderInstructionMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySamusEaterHeaderField), () => VerifySamusEaterHeaderField(rom, false));

    /// <summary>Checks floor versus ceiling mounting against native setup and held-Y direction.</summary>
    private static void VerifySamusEaterMountingMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySamusEaterHeaderField), () => VerifySamusEaterHeaderField(rom, true));

    /// <summary>Checks header selection across all 16-bit identities and one native setup field.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus eater header field")]
    private static void VerifySamusEaterHeaderField(SuperMetroidAddressSpace rom, bool mounting)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort header = (ushort)raw;
            if (header is not ((ushort)PlmHeaderId.InsideReactionBrinstarFloorPlant or (ushort)PlmHeaderId.InsideReactionBrinstarCeilingPlant))
            {
                AssertThrows<InvalidDataException>(() => SamusEaterPlmDefinitions.Resolve((PlmHeaderId)header),
                    "plant header selector rejects every unsupported identity");
                continue;
            }
            var actual = SamusEaterPlmDefinitions.Resolve((PlmHeaderId)header);
            AssertEqual(header == (ushort)PlmHeaderId.InsideReactionBrinstarFloorPlant ? SamusEaterPlmDefinitions.Floor : SamusEaterPlmDefinitions.Ceiling,
                actual, "plant selected header identity");
            if (mounting)
            {
                ushort setup = ReadSamusEaterPlmWord(rom, 0x840000 | header);
                AssertTrue(setup is 0xb0dc or 0xb113, "plant native setup belongs to floor/ceiling pair");
                AssertEqual(setup == 0xb113, actual.Ceiling, "plant mounting follows native setup");
                // The native ceiling subtracts radius and increments held Y; floor adds
                // radius, decrements the edge and decrements held Y.
                AssertEqual((byte)(actual.Ceiling ? 0xed : 0x6d),
                    rom.ReadCartridgeByte(0x840000 | (setup + 4)), "plant native edge add/subtract opcode");
                AssertEqual((byte)(actual.Ceiling ? 0x1a : 0x3a),
                    rom.ReadCartridgeByte(actual.Ceiling ? 0x84b13f : 0x84b10c), "plant native held-Y direction");
            }
            else
            {
                ushort list = ReadSamusEaterPlmWord(rom, 0x840000 | (header + 2));
                AssertEqual(RoomPlmNativeListAddresses.SamusEaterEntries.Single(entry => entry.Entry == list).Program,
                    actual.Program, "plant native initial list selects its typed program");
            }
        }
    }

    /// <summary>Reads a native bank-$84 PLM word for independent header and program comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus eater PLM words")]
    internal static ushort ReadSamusEaterPlmWord(SuperMetroidAddressSpace bus, int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
}
