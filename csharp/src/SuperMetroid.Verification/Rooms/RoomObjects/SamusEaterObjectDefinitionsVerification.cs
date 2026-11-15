using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>Checks the floor and ceiling Samus Eater PLM headers, instruction programs, and live trigger spawns.</summary>
internal static class SamusEaterObjectDefinitionsVerification
{
    /// <summary>Compares native header and program data and checks aligned floor and ceiling plant activation.</summary>
    internal static void VerifySamusEaterObjectDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifySamusEaterHeaderInstructionMapping), () => VerifySamusEaterHeaderInstructionMapping(rom));
        Suite(nameof(VerifySamusEaterMountingMapping), () => VerifySamusEaterMountingMapping(rom));

        foreach ((SamusEaterObjectDefinition definition, _) in Plants)
        {
            const int width = 4;
            const int height = 4;
            int blockCount = width * height;
            var levelWords = new RoomTerrainBlock[blockCount];
            levelWords[5] = RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SpecialBlock);
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(levelWords, new RoomBlockBehavior[blockCount], new RoomBlockAppearance[blockCount]), RoomLevelData.NothingBeyondGrid(), []);
            var roomObjects = RoomObjectFixtures.Unbound();
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
            samus.Position = samus.Position.WithWhole(new(24, definition.Ceiling ? (ushort)16 : (ushort)0));
            samus.Kinematics.Radius = new(8, 16);

            RoomCollisionBlock block = level.GetCollisionBlock(new(1, 1));
            roomObjects.SamusEater.TrySpawnSamusEater(level, block, definition, samus.RoomView);

            RoomObjectPlaceSnapshot place = roomObjects.PopulationPlaces.Single();
            AssertEqual(0, place.UpdatePosition, "Samus Eater takes the first free slot in processing order");
            AssertEqual(new Position16<RoomBlocks>(1, 1), place.Block, "Samus Eater trigger block");
            AssertTrue(place.Behaviour is SamusEaterObject { Phase: SamusEaterPhase.Biting, Holding: false } eater && eater.Plant == definition,
                "Samus Eater starts its typed bite");
            AssertEqual(
                RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock),
                level.GetCollisionBlock(new(1, 1)).Terrain,
                "Samus Eater deactivates its trigger");
        }

        Suite(nameof(VerifySamusEaterChewTimeline), () => VerifySamusEaterChewTimeline());
        Console.WriteLine(
            "Samus Eater PLMs: both header/program identities, the typed programs, and real aligned spawns match cartridge.");
    }

    /// <summary>
    /// Plays each caught plant through to its departure (#1994): from its first update it shows its chew's images, each for
    /// its updates, its plant's number of chews, holding Samus throughout; then it releases her and shows its fangs out for
    /// its release time and its idle image for one update, and leaves the room on the next update.
    /// </summary>
    internal static void VerifySamusEaterChewTimeline()
    {
        InstalledRoomObjectSamusEaterVisuals pictures = InstalledRoomObjectSamusEaterVisuals.Stock();
        foreach ((SamusEaterObjectDefinition definition, _) in Plants)
        {
            const int size = 8;
            var plantBlock = new Position16<RoomBlocks>(3, 3);
            var level = new RoomLevelData(new Extent16<RoomBlocks>(size, size),
                new RoomLevelPlanes(new RoomTerrainBlock[size * size], new RoomBlockBehavior[size * size], new RoomBlockAppearance[size * size]),
                RoomLevelData.NothingBeyondGrid(), []);
            level.SetTerrain(plantBlock, RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SpecialBlock));
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
            samus.Position = samus.Position.WithWhole(new(56, definition.Ceiling ? (ushort)16 : (ushort)0));
            samus.Kinematics.Radius = new(8, 16);
            RoomObjectSystem roomObjects = RoomObjectFixtures.Bound(
                new RoomObjectRoom(level, new SharedGameState(new SharedRandom()), AreaId.Brinstar),
                RoomObjectServiceFixtures.Uninstalled with { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags() },
                RoomObjectFixtures.Unbound());
            roomObjects.SamusEater.TrySpawnSamusEater(level, level.GetCollisionBlock(plantBlock), definition, samus.RoomView);

            var expected = new List<(SamusEaterDraw Image, bool Holding)>();
            for (int chew = 0; chew < definition.Chews; chew++)
                foreach (SamusEaterChewFrame frame in definition.Chew)
                    expected.AddRange(Enumerable.Repeat((frame.Image, true), frame.Updates));
            expected.AddRange(Enumerable.Repeat((definition.Released, false), SamusEaterObjectDefinitions.ReleaseDuration));
            expected.Add((definition.Idle, false));

            for (int update = 0; update < expected.Count; update++)
            {
                roomObjects.Step(level);
                string at = $"{(definition.Ceiling ? "ceiling" : "floor")} plant update {update + 1}";
                AssertTrue(roomObjects.PopulationPlaces.Single().Behaviour is SamusEaterObject { } eater && eater.Holding == expected[update].Holding,
                    $"{at} holds Samus while it chews");
                AssertEqual(pictures.Image(RoomObjectDraw.Of(expected[update].Image)).At(Offset16<RoomBlocks>.Zero),
                    level.GetCollisionBlock(plantBlock).Terrain.Appearance, $"{at} shows {expected[update].Image}");
            }
            roomObjects.Step(level);
            AssertEqual(0, roomObjects.PopulationPlaces.Count, $"{(definition.Ceiling ? "ceiling" : "floor")} plant leaves after its idle image");
        }
    }

    /// <summary>Checks both plant headers' native initial instruction pointers.</summary>
    private static void VerifySamusEaterHeaderInstructionMapping(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifySamusEaterHeaderField), () => VerifySamusEaterHeaderField(rom, false));

    /// <summary>Checks floor versus ceiling mounting against native setup and held-Y direction.</summary>
    private static void VerifySamusEaterMountingMapping(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifySamusEaterHeaderField), () => VerifySamusEaterHeaderField(rom, true));

    /// <summary>
    /// The two plants, floor first, each with the inside reaction that places it (as Samus's inside-block reactions
    /// place it): the cartridge's header for that reaction is the plant's header.
    /// </summary>
    private static readonly (SamusEaterObjectDefinition Plant, SpecialAirInsideReaction Reaction)[] Plants =
    [
        (SamusEaterObjectDefinitions.Floor, SpecialAirInsideReaction.BrinstarFloorPlant),
        (SamusEaterObjectDefinitions.Ceiling, SpecialAirInsideReaction.BrinstarCeilingPlant),
    ];

    /// <summary>Checks each plant's native header: its program, or its mounting against the native setup and held-Y direction.</summary>
    private static void VerifySamusEaterHeaderField(CartridgeImportAddressSpace rom, bool mounting)
    {
        foreach ((SamusEaterObjectDefinition actual, SpecialAirInsideReaction reaction) in Plants)
        {
            ushort header = SpecialAirReactionNativeWords.InsideHeaders[reaction];
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
    internal static ushort ReadSamusEaterObjectWord(CartridgeImportAddressSpace bus, int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
}
