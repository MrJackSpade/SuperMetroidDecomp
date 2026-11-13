using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>Checks the floor and ceiling Samus Eater PLM headers, instruction programs, and live trigger spawns.</summary>
internal static class SamusEaterObjectDefinitionsVerification
{
    /// <summary>Compares native header and program data and checks aligned floor and ceiling plant activation.</summary>
    internal static void VerifySamusEaterObjectDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifySamusEaterHeaderInstructionMapping), () => VerifySamusEaterHeaderInstructionMapping(rom));
        Suite(nameof(VerifySamusEaterMountingMapping), () => VerifySamusEaterMountingMapping(rom));
        AssertEqual((ushort)0xb6cb, RoomObjectNativeWords.Word(SamusEaterObjectDefinitions.Floor.Identity), "floor plant carries the floor plant header");
        AssertEqual((ushort)0xb6cf, RoomObjectNativeWords.Word(SamusEaterObjectDefinitions.Ceiling.Identity), "ceiling plant carries the ceiling plant header");

        foreach (SamusEaterObjectDefinition definition in Plants)
        {
            const int width = 4;
            const int height = 4;
            int blockCount = width * height;
            var levelWords = new RoomTerrainBlock[blockCount];
            levelWords[5] = RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SpecialBlock);
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(levelWords, new RoomBlockBehavior[blockCount], new RoomBlockAppearance[blockCount]), RoomLevelData.NothingBeyondGrid());
            var roomObjects = RoomObjectFixtures.Unbound();
            var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
            samus.Position = samus.Position.WithWhole(new(24, definition.Ceiling ? (ushort)16 : (ushort)0));
            samus.Kinematics.Radius = new(8, 16);

            RoomCollisionBlock block = level.GetCollisionBlock(new(1, 1));
            roomObjects.SamusEater.TrySpawnSamusEater(level, block, definition, samus.RoomView);

            RoomObjectSlotSnapshot slot = roomObjects.PopulationSlots.Single();
            AssertEqual(0, slot.ProcessingPosition, "Samus Eater takes the first free slot in processing order");
            AssertEqual(definition.Identity, slot.HeaderPointer, "Samus Eater header identity");
            AssertEqual(new Position16<RoomBlocks>(1, 1), slot.Block, "Samus Eater trigger block");
            AssertTrue(slot.Behaviour is SamusEaterObject { Phase: SamusEaterPhase.Biting, Holding: false } eater && eater.Plant == definition,
                "Samus Eater starts its typed bite");
            AssertEqual(
                RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock),
                level.GetCollisionBlock(new(1, 1)).Terrain,
                "Samus Eater deactivates its trigger");
        }

        Console.WriteLine(
            "Samus Eater PLMs: both header/program identities, the typed programs, and real aligned spawns match cartridge.");
    }

    /// <summary>Checks both plant headers' native initial instruction pointers.</summary>
    private static void VerifySamusEaterHeaderInstructionMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySamusEaterHeaderField), () => VerifySamusEaterHeaderField(rom, false));

    /// <summary>Checks floor versus ceiling mounting against native setup and held-Y direction.</summary>
    private static void VerifySamusEaterMountingMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySamusEaterHeaderField), () => VerifySamusEaterHeaderField(rom, true));

    /// <summary>The two plants, floor first.</summary>
    private static readonly SamusEaterObjectDefinition[] Plants = [SamusEaterObjectDefinitions.Floor, SamusEaterObjectDefinitions.Ceiling];

    /// <summary>Checks each plant's native header: its program, or its mounting against the native setup and held-Y direction.</summary>
    private static void VerifySamusEaterHeaderField(SuperMetroidAddressSpace rom, bool mounting)
    {
        foreach (SamusEaterObjectDefinition actual in Plants)
        {
            ushort header = RoomObjectNativeWords.Word(actual.Identity);
            if (mounting)
            {
                ushort setup = ReadSamusEaterObjectWord(rom, 0x840000 | header);
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
                RoomObjectProgramsVerification.VerifySamusEaterList(rom, ReadSamusEaterObjectWord(rom, 0x840000 | (header + 2)), actual);
            }
        }
    }

    /// <summary>Reads a native bank-$84 PLM word for independent header and program comparison.</summary>
    internal static ushort ReadSamusEaterObjectWord(SuperMetroidAddressSpace bus, int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
}
