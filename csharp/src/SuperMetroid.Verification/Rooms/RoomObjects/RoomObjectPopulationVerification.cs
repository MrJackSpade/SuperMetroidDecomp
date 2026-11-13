using SuperMetroid.Core.Enemies.SaveStation;
using SuperMetroid.AssetExtraction;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.RoomObjects.CollectiblesVerification;
using static SuperMetroid.Verification.Areas.Escape.EscapeGateVisualsVerification;
using static SuperMetroid.Verification.Enemies.Elevator.LookupElevatorPlatformLayoutsVerification;
using static SuperMetroid.Verification.Enemies.Elevator.LookupElevatorPlatformProgramVerification;
using static SuperMetroid.Verification.Areas.Escape.LookupEscapeGateVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.LookupSpeedEscapeStagesVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyFullMovieVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Artwork.SamusRenderingVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.RoomObjects;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>Checks native PLM population order, resident doors, stations and escape-room controllers.</summary>
internal static class RoomObjectPopulationVerification
{
    /// <summary>Compares the elevator control list, draw selection and collision layouts against the retail cartridge.</summary>
    internal static void VerifyElevatorPlatformObjectDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyElevatorPlatformControls), () => VerifyElevatorPlatformControls(rom));
        Suite(nameof(VerifyElevatorPlatformDrawSelection), () => VerifyElevatorPlatformDrawSelection(rom));
        Suite(nameof(VerifyElevatorPlatformLoopTarget), () => VerifyElevatorPlatformLoopTarget(rom));
        Suite(nameof(VerifyElevatorPlatformCycle), () => VerifyElevatorPlatformCycle(rom));

        Suite(nameof(VerifyElevatorPlatformLayoutGeometry), () => VerifyElevatorPlatformLayoutGeometry(rom));
        Suite(nameof(VerifyElevatorPlatformLayoutCollision), () => VerifyElevatorPlatformLayoutCollision(rom));
        Suite(nameof(VerifyElevatorPlatformLayoutVisuals), () => VerifyElevatorPlatformLayoutVisuals(rom));
        Console.WriteLine("Elevator platform PLM: native instruction loop and three complete draw lists match cartridge.");
    }

    /// <summary>
    /// The typed platform's loop plays as its native list does (#1554): each layout's updates and draw in order, then the
    /// goto back to the list's start.
    /// </summary>
    private static void VerifyElevatorPlatformCycle(CartridgeImportAddressSpace rom)
    {
        const ushort loop = 0xafb6;
        var cartridge = CartridgeImportSource.Require(rom);
        ushort cursor = loop;
        foreach (TerrainChangeFrame frame in ElevatorPlatformObjectDefinitions.Cycle)
        {
            AssertEqual((ushort)frame.Updates, cartridge.ReadCartridgeWord(0x840000 | cursor), $"elevator platform image at $84:{cursor:X4} stays its updates");
            AssertEqual(RoomObjectDrawAddressCodec.Of(frame.Draw), cartridge.ReadCartridgeWord(0x840000 | (cursor + 2)), $"elevator platform image at $84:{cursor:X4} draws its layout");
            cursor = unchecked((ushort)(cursor + 4));
        }
        AssertEqual((ushort)RoomObjectInstruction.Goto, cartridge.ReadCartridgeWord(0x840000 | cursor), "elevator platform loop goes back");
        AssertEqual(loop, cartridge.ReadCartridgeWord(0x840000 | (cursor + 2)), "elevator platform loop goes back to its first image");
    }

    /// <summary>Entries in <c>Door_Closing_PLMs</c> ($8F:E68A), the orientation byte's domain.</summary>
    private const byte DoorClosingTableEntries = 12;

    /// <summary>
    /// Checks all orientation bytes, including out-of-table rejection: each closer begins the initial list of the header
    /// the bank-$8F table names for its orientation, and a non-closing entry selects none. The header itself is not
    /// compared, as the game keeps no record of it (#1691).
    /// </summary>
    private static void VerifyFallbackDoorLists(CartridgeImportAddressSpace rom)
    {
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            byte direction = (byte)raw;
            if (raw >= DoorClosingTableEntries)
            {
                AssertThrows<InvalidDataException>(() => DoorRecordCodec.DecodeOrientation(direction), "Orientation beyond Door_Closing_PLMs rejected");
                continue;
            }
            ushort header = ReadSamusEaterObjectWord(rom, 0x8fe68a + raw * 2);
            DoorOrientation orientation = DoorRecordCodec.DecodeOrientation(direction);
            AssertEqual(direction, orientation.Encode(), "Orientation byte round-trips");
            var actual = DoorClosingObjects.GetDefinition(orientation);
            AssertEqual(header == 0, actual is null, "Non-closing table entries select no actor");
            AssertEqual(header == 0 ? (ushort)0 : ReadSamusEaterObjectWord(rom, 0x840000 | (header + 2)),
                actual is { } fallback ? TypedDoorObjectOracle.ClosingListOf(fallback) : (ushort)0,
                "Fallback original initial list or no-actor zero");
        }
    }

    /// <summary>Checks compiled door-closing definitions and their admission through actual door PLM allocation.</summary>
    internal static void VerifyDoorClosingObjectDefinitions(CartridgeImportAddressSpace rom, bool fallbackOnly = false)
    {

        Suite(nameof(VerifyFallbackDoorLists), () => VerifyFallbackDoorLists(rom));
        for (byte direction = 0; direction < DoorClosingTableEntries; direction++)
        {
            ushort expectedHeader = CartridgeImportSource.Require(rom).ReadCartridgeWord((DoorClosingObjects.HeaderTableAddress + direction * sizeof(ushort)));
            ushort expectedList = expectedHeader == 0
                ? (ushort)0
                : CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | unchecked((ushort)(expectedHeader + 2))));
            var roomObjects = RoomObjectFixtures.Unbound();
            RoomLevelData level = CreateRoom(new(4, 4),
                new RoomTerrainBlock[16],
                new RoomBlockBehavior[16]);
            var door = new DoorDefinition(
                Name: "fixture door",
                DestinationRoom: FixtureRooms.UnloadedDestination,
                Orientation: DoorRecordCodec.DecodeOrientation(direction),
                RoomObjectBlock: new(1, 1),
                DestinationScreen: new(0, 0),
                SamusSpeed: DoorSamusSpeed.None,
                SetupCode: DoorSetupCode.None);
            bool spawned = roomObjects.DoorObjects.TrySpawnDoorClosingObject(
                level, door, new SharedGameState(new SharedRandom()));
            AssertEqual(expectedHeader != 0, spawned,
                $"door-closing direction {direction} fallback admission matches cartridge");
            if (expectedHeader == 0)
                continue;
            RoomObjectSlotSnapshot slot = roomObjects.PopulationSlots.Single();
            AssertEqual(expectedList, TypedDoorObjectOracle.StartListOf(slot.Behaviour),
                $"door-closing direction {direction} starts the closing of its compiled initial list");
        }

        AssertThrows<InvalidDataException>(
            () => DoorRecordCodec.DecodeOrientation(DoorClosingTableEntries),
            "out-of-range door orientation fails loudly");
        if (fallbackOnly) return;
        Suite(nameof(VerifyResidentDoorClosingDefinitions), () => VerifyResidentDoorClosingDefinitions(rom));
        Suite(nameof(VerifyMotherBrainEscapeGateCompiledDefinitions), () => VerifyMotherBrainEscapeGateCompiledDefinitions(rom));
        Suite(nameof(VerifyEscapeGateVisuals), () => VerifyEscapeGateVisuals(rom));
        Suite(nameof(VerifySequentialRoomObjectPopulationLoader), () => VerifySequentialRoomObjectPopulationLoader());
        Console.WriteLine(
            "Door-closing definitions: all twelve fallback and eighteen resident selections match.");
    }

    /// <summary>
    /// Verifies the fixed header+4 closing list of every resident door: a typed cap's or the escape gate's closing begins
    /// exactly there, and an eye door's body, whose closing list is the delete, leaves the room on its next update. Each
    /// then runs the real room-population load and transition redirect with that source word absent from the sparse bus.
    /// A room object at the door that has no closing fails loudly.
    /// </summary>
    internal static void VerifyResidentDoorClosingDefinitions(CartridgeImportAddressSpace rom)
    {
        ushort[] headers = [0xbaf4,0xc842,0xc848,0xc84e,0xc854,0xc85a,0xc860,0xc866,0xc86c,
            0xc872,0xc878,0xc87e,0xc884,0xc88a,0xc890,0xc896,0xc89c,0xc8ca,0xdb4c,0xdb5a];

        int definitionIndex = 0;
        foreach (ushort raw in headers)
        {
            RoomObjectKind header = RoomObjectNativeWords.Header(raw);
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | unchecked((ushort)(raw + 4))));
            bool eyeDoorBody = header is RoomObjectKind.EyeDoorFacingRight or RoomObjectKind.EyeDoorFacingLeft;
            ushort typedClosing = DoorObjectNativeListAddresses.TypedClosingEntry(header)
                ?? (header == RoomObjectKind.MotherBrainEscapeRoomGate ? DoorObjectNativeListAddresses.EscapeGateClosing
                    : eyeDoorBody ? RoomObjectNativeListAddresses.DeleteList
                    : throw new InvalidOperationException($"Resident door $84:{raw:X4} has no typed closing."));
            AssertEqual(expected, typedClosing, $"resident door $84:{raw:X4} closing begins at its header's list");

            // Typed placement metadata supplies the first list only. The transition
            // must obtain its closing program from the resident cap or catalog,
            // not from an arbitrary address space or a header+4 fixture byte.
            var bus = new TestAddressSpace();
            const ushort population = 0x9000;
            byte blockX = (byte)(1 + definitionIndex % 4);
            byte blockY = (byte)(1 + definitionIndex / 4);
            var decoded = new RoomObjectPopulation($"$8F:{population:X4}",
            [
                // Two directional caps are not placed in any retail room population,
                // so they are absent from the 70-header population catalog.
                new RoomObjectPlacement(header,
                    new(blockX, blockY), RoomObjectRoomArgumentWords.Decode(header, 0x8000)), // Negative argument always closes.
            ]);

            RoomLevelData level = CreateRoom(new(8, 8),
                new RoomTerrainBlock[64],
                new RoomBlockBehavior[64]);
            var system = new SharedGameState(new SharedRandom());
            var roomObjects = RoomObjectFixtures.Unbound();
            AssertEqual(1, RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, system, AreaId.Crateria), decoded, new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
                $"resident door $84:{raw:X4} loads through production setup");

            var enteringDoor = new DoorDefinition(
                Name: "fixture door",
                DestinationRoom: FixtureRooms.UnloadedDestination,
                Orientation: new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses),
                RoomObjectBlock: new(blockX, blockY),
                DestinationScreen: new(0, 0),
                SamusSpeed: DoorSamusSpeed.None,
                SetupCode: DoorSetupCode.None);
            AssertTrue(roomObjects.DoorObjects.TrySpawnDoorClosingObject(level, enteringDoor, system),
                $"resident door $84:{raw:X4} accepts transition redirect");
            RoomObjectSlotSnapshot slot = roomObjects.PopulationSlots.Single();
            AssertTrue(TypedDoorObjectOracle.IsClosing(slot.Behaviour), $"resident door $84:{raw:X4} starts its closing");
            if (eyeDoorBody)
            {
                roomObjects.Step(level);
                AssertEqual(0, roomObjects.SlotTable.ActiveCount, $"resident eye door body $84:{raw:X4} leaves on its next update");
            }
            definitionIndex++;
        }

        // A shot blue cap at the door's block has no closing: the transition fails loudly rather than skipping it.
        RoomLevelData blueLevel = CreateRoom(new(8, 8), new RoomTerrainBlock[64], new RoomBlockBehavior[64]);
        var blueObjects = RoomObjectFixtures.Unbound();
        AssertTrue(blueObjects.DoorObjects.TrySpawnBlueDoorOpening(blueLevel, new(1, 1),
                RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(ColoredDoorOrientation.Left)), SamusProjectileType.None),
            "a shot blue cap stands at the door's block");
        var blueDoor = new DoorDefinition(
            Name: "fixture door",
            DestinationRoom: FixtureRooms.UnloadedDestination,
            Orientation: new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses),
            RoomObjectBlock: new(1, 1),
            DestinationScreen: new(0, 0),
            SamusSpeed: DoorSamusSpeed.None,
            SetupCode: DoorSetupCode.None);
        AssertThrows<InvalidDataException>(
            () => blueObjects.DoorObjects.TrySpawnDoorClosingObject(blueLevel, blueDoor, new SharedGameState(new SharedRandom())),
            "a shot blue cap at the door is outside the resident closing domain");
    }

    /// <summary>Compares the three Speed Booster escape stages with retail control data and their runtime handoffs.</summary>
    internal static void VerifySpeedBoosterEscapeStageDefinitions(
        CartridgeImportAddressSpace rom)
    {
        Suite(nameof(RoomObjectProgramsVerification.VerifySpeedBoosterEscapePrograms), () => RoomObjectProgramsVerification.VerifySpeedBoosterEscapePrograms(rom));
        Suite(nameof(VerifySpeedEscapeStageTargets), () => VerifySpeedEscapeStageTargets(rom));
        Suite(nameof(VerifySpeedEscapeStageHeights), () => VerifySpeedEscapeStageHeights(rom));
        Suite(nameof(VerifySpeedEscapeStageVelocities), () => VerifySpeedEscapeStageVelocities(rom));

        // This fresh bus contains the PLM program and FX fixture, but deliberately omits
        // $84:B876-$B889. The production controller must finish all three physical stages.
        Suite(nameof(VerifySpeedBoosterEscapeObject), () => VerifySpeedBoosterEscapeObject(new TestAddressSpace()));
        Console.WriteLine(
            "Speed Booster escape definitions: three physical stages and the terminal event match.");
    }

    /// <summary>
    /// Locks the single bank-$8F parser, descending physical IDs, synchronous slot reuse,
    /// elevator animation, and all requested station setup/contact families to bounded
    /// synthetic rooms. No assertion crosses a door or depends on frontend choreography.
    /// </summary>
    internal static void VerifySequentialRoomObjectPopulationLoader()
    {
        var bus = new TestAddressSpace();
        SeedRoomObjectPopulationRom(bus);
        SeedDormantScrollObjectProgram(bus);
        const ushort population = 0x9000;
        SeedRoomObjectPopulation(bus, population, [
            // First resident takes native slot 39.
            0x03, 0xb7, 0x08, 0x04, 0x00, 0x91,
            // Setup deletes this temporary extension synchronously.
            0x3b, 0xb6, 0x09, 0x04, 0x00, 0x80,
            // It must reuse slot 38, not receive a slot based on family load order.
            0x0b, 0xb7, 0x0c, 0x08, 0x00, 0x00,
            // The following resident then receives slot 37.
            0xdf, 0xb6, 0x14, 0x08, 0x00, 0x00,
            0x00, 0x00,
        ]);

        const int width = 32;
        const int height = 16;
        RoomLevelData level = CreateRoom(new(width, height),
            new RoomTerrainBlock[width * height],
            new RoomBlockBehavior[width * height]);
        var system = new SharedGameState(new SharedRandom());
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Health = 25,
            MaxHealth = 199,
            Missiles = 1,
            MaxMissiles = 15,
        };
        var roomObjects = RoomObjectFixtures.Unbound();
        int parsed = RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, system, AreaId.Crateria), RoomObjectPopulationImporter.Read(bus, population), new RoomObjectServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });

        AssertEqual(4, parsed, "single parser consumes every six-byte record once");
        RoomObjectSlotSnapshot[] slots = roomObjects.PopulationSlots.ToArray();
        AssertEqual(3, slots.Length, "extension setup deletes its preallocated actor");
        AssertEqual(0, slots[0].ProcessingPosition, "first record is processed first");
        AssertTrue(slots[0].Behaviour is ScrollTriggerObject, "first resident holds its record's scroll trigger");
        AssertEqual(1, slots[1].ProcessingPosition, "post-delete record reuses the next slot in processing order");
        AssertTrue(slots[1].Behaviour is ElevatorPlatformObject, "elevator follows ROM order, not family order");
        AssertEqual(2, slots[2].ProcessingPosition, "later station is processed after them");
        AssertTrue(slots[2].Behaviour is StationObject { Kind: StationKind.Energy }, "the later record's energy station is resident");
        AssertEqual(1, roomObjects.ElevatorPlatforms.Count, "elevator platform family is resident");
        AssertEqual(RoomTerrainBlock.Of(0x000, BlockMirroring.None, RoomCollisionType.Air), level.GetCollisionBlock(new(12, 8)).Terrain,
            "elevator setup clears collision bits through shared setup dispatcher");

        // Energy right access is parent+1 and retains solid type B while publishing the
        // parent trigger. The next PLM pass refills through live Samus state and emits the
        // cartridge message ID; it does not require a room-name callback.
        AssertEqual(RoomTerrainBlock.Of(0x000, BlockMirroring.None, RoomCollisionType.SpecialBlock), level.GetCollisionBlock(new(21, 8)).Terrain,
            "energy setup installs type-B right access");
        AssertEqual(0x49, RoomBlockBehaviorCodec.Encode(level.GetCollisionBlock(new(21, 8)).Behavior),
            "energy setup installs BTS $49");
        AssertTrue(roomObjects.TryNotifyStationTouch(level, new(21, 8), 0x49),
            "generic BTS dispatcher locates energy-station parent");
        AssertTrue(samus.Control.AlphaLocked,
            "station access setup runs command six before its first PLM handler pass");
        roomObjects.Step(level);
        AssertTrue(samus.Control.AlphaLocked,
            "station access command locks Samus during six-plus-$60 insertion");
        AssertTrue(roomObjects.Sounds.SoundRequests.Contains(new SoundRequest(EnemySound.StationEngaged, SoundRequestPriority.Normal)),
            "station access begins with cartridge extension sound $37");
        // The triggering pass installs the six-frame timer without decrementing it, so the
        // activation opcode runs on pass 1 + 6 + $60 = 103.
        for (int frame = 1; frame < 102; frame++)
            roomObjects.Step(level);
        AssertEqual(25, samus.Health, "energy station has not activated before its 103rd pass");
        roomObjects.Step(level);
        AssertEqual(199, samus.Health, "energy station restores health to cartridge maximum");
        AssertEqual(1, roomObjects.Stations.StationActivationEvents.Count,
            "energy station publishes one shared message request");
        AssertEqual(GameplayMessageIds.EnergyRechargeCompleted,
            roomObjects.Stations.StationActivationEvents[0].MessageBoxIndex,
            "energy station uses cartridge message $15");

        Suite(nameof(VerifyOtherStationFamilies), () => VerifyOtherStationFamilies(bus));
        Suite(nameof(VerifySaveStationConfirmation), () => VerifySaveStationConfirmation(bus));
        Suite(nameof(VerifyNativeOutOfBoundsPopulationSetupOrder), () => VerifyNativeOutOfBoundsPopulationSetupOrder());
        Suite(nameof(VerifyMetroidsClearedStateObject), () => VerifyMetroidsClearedStateObject(bus));
        Suite(nameof(VerifyShaktoolRoomObject), () => VerifyShaktoolRoomObject());
        Suite(nameof(VerifyMotherBrainEscapeRoomGate), () => VerifyMotherBrainEscapeRoomGate(bus));
        Suite(nameof(VerifyBombTorizoGreyDoorClosingReentry), () => VerifyBombTorizoGreyDoorClosingReentry(bus));
        Suite(nameof(VerifyStandardGreyDoorClosingFallthrough), () => VerifyStandardGreyDoorClosingFallthrough(bus));
        Suite(nameof(VerifySpeedBoosterEscapeObject), () => VerifySpeedBoosterEscapeObject(bus));
        Suite(nameof(VerifyWreckedShipAtticObject), () => VerifyWreckedShipAtticObject(bus));
        Suite(nameof(VerifyUnsupportedPopulationContext), () => VerifyUnsupportedPopulationContext(bus));
        Console.WriteLine(
            "  Room PLM population: one-pass native slots, synchronous reuse, elevator, and station families agree.");
    }

    /// <summary>
    /// Reproduces the `$BAF4` resident-door path from room `$9804` using the
    /// compiled retail instructions with a synthetic population/header. Bombs
    /// admit the closing animation, then its final Goto returns to `$BA7F`.
    /// </summary>
    private static void VerifyBombTorizoGreyDoorClosingReentry(TestAddressSpace bus)
    {
        const ushort population = 0x9690;
        const int width = 16;
        const int height = 16;
        const int doorX = 1;
        const int doorY = 6;
        const ushort roomArgument = 0x081b;
        const ushort initialList = 0xba7f;
        const ushort closingList = 0xba4c;

        SeedObjectProgramWords(bus, RoomObjectNativeWords.Word(RoomObjectKind.BombTorizoGreyDoor), 0xc794);
        SeedObjectProgramWords(bus, unchecked((ushort)(RoomObjectNativeWords.Word(RoomObjectKind.BombTorizoGreyDoor) + 2)),
            initialList);
        SeedObjectProgramWords(bus, unchecked((ushort)(RoomObjectNativeWords.Word(RoomObjectKind.BombTorizoGreyDoor) + 4)),
            closingList);

        // The PLM program and its draw pointers are compiled cartridge data. Do
        // not inject a shortened stand-in at their native addresses: the timing
        // and Bombs callback need to execute the same lists used in gameplay.

        SeedRoomObjectPopulation(bus, population,
        [
            0xf4, 0xba, doorX, doorY,
            unchecked((byte)roomArgument), LittleEndianBytes.HighByte(roomArgument),
            0x00, 0x00,
        ]);
        RoomLevelData level = CreateRoom(new(width, height),
            new RoomTerrainBlock[width * height],
            new RoomBlockBehavior[width * height]);
        var system = new SharedGameState(new SharedRandom());
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            CollectedItems = SamusEquipmentFlags.Bombs,
        };
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertEqual(1, RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, system, AreaId.Crateria), RoomObjectPopulationImporter.Read(bus, population), new RoomObjectServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "Bomb Torizo grey door loads through the shared room population");

        var enteringDoor = new DoorDefinition(
            Name: nameof(CrateriaDoorDefinitions.Flyway1),
            DestinationRoom: RoomDefinitions.BombTorizo,
            Orientation: new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses),
            RoomObjectBlock: new(doorX, doorY),
            DestinationScreen: new(0, 0),
            SamusSpeed: DoorSamusSpeed.AxisDefault,
            SetupCode: DoorSetupCode.None);
        AssertTrue(roomObjects.DoorObjects.TrySpawnDoorClosingObject(level, enteringDoor, system),
            "Bomb Torizo entry redirects the resident door to its secondary list");
        AssertEqual(GreyDoorPhase.Closing, roomObjects.GreyDoorSnapshots.Single().Phase,
            "redirected actor retains explicit grey-door ownership while closing");

        roomObjects.Step(level, new RoomObjectFrameInputs { Scrolls = null, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0, CollectedItems = samus.CollectedItems });
        AssertEqual(RoomTerrainBlock.Of(0x082, BlockMirroring.Horizontal, RoomCollisionType.Air), level.GetCollisionBlock(new(doorX, doorY)).Terrain,
            "collected Bombs admit the secondary list's closing draw");

        for (int frame = 0; frame < 80 &&
             roomObjects.GreyDoorSnapshots.Single().Phase == GreyDoorPhase.Closing; frame++)
            roomObjects.Step(level, new RoomObjectFrameInputs { Scrolls = null, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0, CollectedItems = samus.CollectedItems });
        AssertEqual(GreyDoorPhase.Locked, roomObjects.GreyDoorSnapshots.Single().Phase,
            "closing Goto hands the resident actor back to its grey-door family");
        AssertEqual(initialList, DoorObjectNativeListAddresses.Grey(new GreyDoorVariant(ColoredDoorOrientation.Right, BombTorizo: true)).Waiting,
            "the locked grey cap's first look stands for the cartridge first list");
        AssertEqual(RoomTerrainBlock.Of(0x0ae, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock), level.GetCollisionBlock(new(doorX, doorY)).Terrain,
            "re-entry draws the locked grey-door frame in the same handler pass");
        AssertEqual(1, roomObjects.SlotTable.ActiveCount,
            "rejoined Bomb Torizo door remains resident for the battle condition");
    }

    /// <summary>
    /// Reproduces issue 236's retail <c>$84:C842</c> grey door in room
    /// <c>$8F:9B9D</c>. Its closing list occupies <c>$BE59..$BE6F</c> and falls directly
    /// into first list <c>$BE70</c>; it has no Goto like Bomb Torizo's special door.
    /// </summary>
    private static void VerifyStandardGreyDoorClosingFallthrough(TestAddressSpace bus)
    {
        const ushort population = 0x96d0;
        const int width = 48;
        const int height = 16;
        const int doorX = 0x2e;
        const int doorY = 6;
        const ushort roomArgument = 0x0c25;
        const ushort initialList = 0xbe70;
        const ushort closingList = 0xbe59;
        const ushort closedBlueList = 0xc100;
        const ushort activationList = 0xbe84;
        const ushort openTriggerList = 0xbea8;
        const ushort openingList = 0xbead;
        const ushort closingDraw = 0xa600;
        const ushort lockedDraw = 0xa610;

        SeedObjectProgramWords(bus, RoomObjectNativeWords.Word(RoomObjectKind.GreyDoorFacingLeft), 0xc794);
        SeedObjectProgramWords(bus, unchecked((ushort)(RoomObjectNativeWords.Word(RoomObjectKind.GreyDoorFacingLeft) + 2)),
            initialList);
        SeedObjectProgramWords(bus, unchecked((ushort)(RoomObjectNativeWords.Word(RoomObjectKind.GreyDoorFacingLeft) + 4)),
            closingList);
        SeedObjectProgramWords(bus, unchecked(initialList + 2), closedBlueList);
        SeedObjectProgramWords(bus, unchecked(initialList + 6), activationList);
        SeedObjectProgramWords(bus, unchecked(initialList + 12), lockedDraw);
        SeedObjectProgramWords(bus, unchecked(activationList + 2), openTriggerList);
        SeedObjectProgramBytes(bus, unchecked(openTriggerList + 2),
        [
            0x01,
            unchecked((byte)openingList),
            LittleEndianBytes.HighByte(openingList),
        ]);

        // Preserve the exact byte widths of BE59: five timer/draw pairs with an odd-sized
        // queue-sound instruction between the second and third pair. The final pair ends
        // at BE70, the start of the resident door's ordinary first list.
        SeedObjectProgramWords(bus, closingList, 2);
        SeedObjectProgramWords(bus, unchecked(closingList + 2), closingDraw);
        SeedObjectProgramWords(bus, unchecked(closingList + 4), 2);
        SeedObjectProgramWords(bus, unchecked(closingList + 6), closingDraw);
        SeedObjectProgramWords(bus, unchecked(closingList + 8),
            (ushort)RoomObjectInstruction.QueueSoundLibrary3Maximum6);
        SeedObjectProgramBytes(bus, unchecked(closingList + 10), [0x08]);
        SeedObjectProgramWords(bus, unchecked(closingList + 11), 2);
        SeedObjectProgramWords(bus, unchecked(closingList + 13), closingDraw);
        SeedObjectProgramWords(bus, unchecked(closingList + 15), 2);
        SeedObjectProgramWords(bus, unchecked(closingList + 17), closingDraw);
        SeedObjectProgramWords(bus, unchecked(closingList + 19), 1);
        SeedObjectProgramWords(bus, unchecked(closingList + 21), closingDraw);
        WriteOneBlockDraw(bus, closingDraw, 0xc123);
        WriteOneBlockDraw(bus, lockedDraw, 0xc123);

        SeedRoomObjectPopulation(bus, population,
        [
            unchecked((byte)RoomObjectNativeWords.Word(RoomObjectKind.GreyDoorFacingLeft)),
            LittleEndianBytes.HighByte(RoomObjectNativeWords.Word(RoomObjectKind.GreyDoorFacingLeft)),
            doorX,
            doorY,
            unchecked((byte)roomArgument),
            LittleEndianBytes.HighByte(roomArgument),
            0x00, 0x00,
        ]);
        RoomLevelData level = CreateRoom(new(width, height),
            new RoomTerrainBlock[width * height],
            new RoomBlockBehavior[width * height]);
        var system = new SharedGameState(new SharedRandom());
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertEqual(1, RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, system, AreaId.Brinstar), RoomObjectPopulationImporter.Read(bus, population), new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "Brinstar pre-map grey door loads through the shared population path");

        var enteringDoor = new DoorDefinition(
            Name: nameof(BrinstarDoorDefinitions.GreenBrinstarMainShaft1),
            DestinationRoom: RoomDefinitions.BrinstarPreMap,
            Orientation: new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses),
            RoomObjectBlock: new(doorX, doorY),
            DestinationScreen: new(0, 0),
            SamusSpeed: DoorSamusSpeed.AxisDefault,
            SetupCode: DoorSetupCode.None);
        AssertTrue(roomObjects.DoorObjects.TrySpawnDoorClosingObject(level, enteringDoor, system),
            "room $9B9D redirects its resident grey door to closing list $BE59");

        for (int frame = 0; frame < 10; frame++)
        {
            roomObjects.Step(level, new RoomObjectFrameInputs { Scrolls = null, EnemyDeaths = 0, EnemyDeathQuota = 1, ControllerNewInput = 0, CollectedItems = 0 });
        }

        AssertEqual(GreyDoorPhase.Locked, roomObjects.GreyDoorSnapshots.Single().Phase,
            "fall-through at $BE70 returns to the resident grey-door owner");
        AssertEqual(initialList, DoorObjectNativeListAddresses.Grey(new GreyDoorVariant(ColoredDoorOrientation.Left, BombTorizo: false)).Waiting,
            "the grey door's first look stands for the cartridge first list");
        AssertTrue(roomObjects.PopulationSlots.Single().Behaviour is GreyDoorObject { Phase: GreyDoorPhase.Locked },
            "the closing runs into the grey door's own first look, which locks it");
        AssertEqual(1, roomObjects.SlotTable.ActiveCount,
            "room $9B9D grey door remains resident after its entry-closing animation");
    }

    /// <summary>
    /// Reproduces the complete $BB05 lifecycle in a bounded synthetic room. Although its
    /// callback is intentionally inert, the actor must retain a native slot, install the
    /// exact $BAFA pre-instruction from ROM, and remain asleep without mutating terrain.
    /// </summary>
    private static void VerifyWreckedShipAtticObject(TestAddressSpace bus)
    {
        const ushort population = 0x9640;
        const int width = 8;
        const int height = 8;
        const int blockX = 3;
        const int blockY = 4;

        SeedObjectProgramWords(bus, unchecked((ushort)(RoomObjectNativeWords.Word(RoomObjectKind.WreckedShipAttic) + 2)),
            RoomObjectNativeListAddresses.WreckedShipAtticList);
        CartridgeImportAddressSpace? rom = File.Exists("Super Metroid.smc")
            ? SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc")
            : null;
        if (rom is not null)
            RoomObjectProgramsVerification.VerifyWreckedShipAtticPrograms(rom);
        for (int index = 0; index < 3; index++)
            SeedObjectProgramWords(bus, checked((ushort)(RoomObjectNativeListAddresses.WreckedShipAtticList + index * 2)), 0xdead);
        SeedRoomObjectPopulation(bus, population,
        [
            0x05, 0xbb, blockX, blockY, 0x34, 0x12,
            0x00, 0x00,
        ]);

        var levelWords = new RoomTerrainBlock[width * height];
        levelWords[blockY * width + blockX] = RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.DoorBlock);
        RoomLevelData level = CreateRoom(new(width, height),
            levelWords,
            new RoomBlockBehavior[width * height]);
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertEqual(1, RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, new SharedGameState(new SharedRandom()), AreaId.WreckedShip), RoomObjectPopulationImporter.Read(bus, population), new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "Wrecked Ship attic record is accepted by sequential loader");
        RoomObjectSlotSnapshot loaded = roomObjects.PopulationSlots.Single();
        AssertEqual(0, loaded.ProcessingPosition,
            "Wrecked Ship attic actor takes the first free slot in processing order");
        AssertTrue(loaded.Behaviour is WreckedShipAtticObject, "Wrecked Ship attic actor is its typed object");

        roomObjects.Step(level);
        AssertTrue(roomObjects.PopulationSlots.Single().Behaviour is WreckedShipAtticObject,
            "Wrecked Ship attic actor holds its place after its first update");
        roomObjects.Step(level);
        AssertEqual(1, roomObjects.SlotTable.ActiveCount,
            "inert attic callback leaves its resident actor alive");
        AssertEqual(RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.DoorBlock), level.GetCollisionBlock(new(blockX, blockY)).Terrain,
            "inert attic callback leaves authored terrain unchanged");
    }

    /// <summary>
    /// Exercises setup $B89C and all three callbacks installed by the cartridge's $B88A
    /// list. The fixture writes the explicit shared FX motion words and compiled stage records,
    /// so the test observes the same shared FX/event/earthquake owners as production.
    /// </summary>
    internal static void VerifySpeedBoosterEscapeObject(TestAddressSpace bus)
    {
        const ushort population = 0x9620;
        const int width = 16;
        const int height = 16;

        SeedObjectProgramWords(bus, 0xb8ae, RoomObjectNativeListAddresses.SpeedBoosterEscapeList);
        // Poison the old cartridge path: all three real handoffs must now use
        // the compiled list even when a synthetic address space disagrees.
        for (int index = 0; index < RoomObjectNativeListAddresses.SpeedBoosterEscapeWordCount; index++)
        {
            SeedObjectProgramWords(bus, unchecked((ushort)(RoomObjectNativeListAddresses.SpeedBoosterEscapeList + index * 2)),
                0xdead);
        }
        SeedRoomObjectPopulation(bus, population,
        [
            0xac, 0xb8, 0x01, 0x01, 0x00, 0x00,
            0x00, 0x00,
        ]);
        RoomLevelData CreateLevel() => CreateRoom(new(width, height),
            new RoomTerrainBlock[width * height],
            new RoomBlockBehavior[width * height]);

        RoomFxOverlayState CreateFx(ushort baseY, int? targetY, ushort velocity, byte timer)
        {
            var fxSession = new RoomFxSession();
            var fx = new RoomFxOverlayState(fxSession);
            fx.SetLiquidMotion(baseY, targetY, new SignedEightEight(velocity), timer);
            if (targetY is null)
                fx.RemoveLiquidTarget();
            return fx;
        }

        RoomObjectSystem Load(
            SharedGameState system,
            SamusState samus,
            RoomFxOverlayState fx,
            RoomLevelData level,
            Action stopEarthquake)
        {
            var roomObjects = RoomObjectFixtures.Unbound();
            AssertEqual(1, RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, system, AreaId.Norfair), RoomObjectPopulationImporter.Read(bus, population, AreaId.Norfair), new RoomObjectServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Events = new FixtureEventFlags { HasEvent = system.HasEvent, SetEvent = system.SetEvent }, RoomFx = new RoomObjectFx(fx, () => samus), Earthquake = new FixtureRoomEarthquake { WriteStop = stopEarthquake } }),
                "Speed Booster escape record is accepted by sequential loader");
            return roomObjects;
        }

        var completedSystem = new SharedGameState(new SharedRandom());
        completedSystem.SetEvent(StoryEvent.OutranSpeedBoosterLavaquake);
        RoomLevelData completedLevel = CreateLevel();
        RoomObjectSystem completed = Load(
            completedSystem,
            new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()),
            CreateFx(0x0300, 0x0100, 0, 5),
            completedLevel,
            () => { });
        AssertEqual(0, completed.SlotTable.ActiveCount,
            "event-$15 setup deletes B8AC synchronously");

        var unequippedSystem = new SharedGameState(new SharedRandom());
        var unequippedSamus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
        RoomFxOverlayState unequippedFx = CreateFx(0x0300, 0x0100, 0x1234, 5);
        bool earthquakeStopped = false;
        RoomLevelData unequippedLevel = CreateLevel();
        RoomObjectSystem unequipped = Load(
            unequippedSystem,
            unequippedSamus,
            unequippedFx,
            unequippedLevel,
            () => earthquakeStopped = true);
        unequipped.Step(unequippedLevel);
        unequipped.Step(unequippedLevel);
        AssertEqual(0, unequipped.SlotTable.ActiveCount,
            "missing Speed Booster deletes the resident controller");
        AssertTrue(unequippedFx.TargetSurface is null,
            "missing Speed Booster removes the FX target");
        AssertEqual(0, unequippedFx.YVelocity.RawValue,
            "missing Speed Booster clears packed FX velocity");
        AssertEqual(0, unequippedFx.Timer.RawValue,
            "missing Speed Booster clears FX timer");
        AssertTrue(earthquakeStopped,
            "missing Speed Booster stops the room's earthquake");

        var disabledSystem = new SharedGameState(new SharedRandom());
        var disabledSamus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            CollectedItems = SamusEquipmentFlags.SpeedBooster,
        };
        RoomLevelData disabledLevel = CreateLevel();
        RoomObjectSystem disabled = Load(
            disabledSystem,
            disabledSamus,
            CreateFx(0x0300, null, 0, 0),
            disabledLevel,
            () => { });
        disabled.Step(disabledLevel);
        disabled.Step(disabledLevel);
        AssertEqual(0, disabled.SlotTable.ActiveCount,
            "negative FX target deletes collected-Speed-Booster controller");

        var activeSystem = new SharedGameState(new SharedRandom());
        var activeSamus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            CollectedItems = SamusEquipmentFlags.SpeedBooster,
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x0ae1, 0)),
        };
        RoomFxOverlayState activeFx = CreateFx(0x0300, 0x0100, 0, 0);
        RoomLevelData activeLevel = CreateLevel();
        RoomObjectSystem active = Load(
            activeSystem,
            activeSamus,
            activeFx,
            activeLevel,
            () => { });
        active.Step(activeLevel);
        active.Step(activeLevel);
        AssertEqual(SpeedBoosterEscapeDefinitions.InitialLavaquakeVelocity.RawValue,
            activeFx.YVelocity.RawValue,
            "collected Speed Booster starts the regional lavaquake velocity");
        active.Step(activeLevel);
        AssertEqual(0, activeFx.Timer.RawValue,
            "Samus right of $0AE0 leaves FX motion sleeping");
        activeSamus.Position = activeSamus.Position.WithWholeX(SpeedBoosterEscapeDefinitions.StartFxMotionSamusX);
        active.Step(activeLevel);
        AssertEqual(1, activeFx.Timer.RawValue,
            "Samus reaching $0AE0 starts FX motion");

        activeSamus.Position = activeSamus.Position.WithWholeX(0x072c);
        active.Step(activeLevel);
        AssertEqual(0x0300, activeFx.NamedBaseY.Whole,
            "first lava stage waits while Samus remains right of its threshold");
        activeSamus.Position = activeSamus.Position.WithWholeX(0x072b);
        active.Step(activeLevel);
        AssertEqual(0x01bf, activeFx.NamedBaseY.Whole,
            "first lava stage clamps maximum FX Y");
        AssertEqual(0xff50, activeFx.YVelocity.RawValue,
            "first lava stage installs ROM velocity");
        activeSamus.Position = activeSamus.Position.WithWholeX(0x050a);
        active.Step(activeLevel);
        AssertEqual(0x0167, activeFx.NamedBaseY.Whole,
            "second lava stage clamps maximum FX Y");
        activeSamus.Position = activeSamus.Position.WithWholeX(0x0244);
        active.Step(activeLevel);
        AssertEqual(0x0100, activeFx.NamedBaseY.Whole,
            "third lava stage clamps maximum FX Y");
        active.Step(activeLevel);
        AssertTrue(activeSystem.HasEvent(StoryEvent.OutranSpeedBoosterLavaquake),
            "passing the last rise marks the escape's event");
        AssertEqual(1, active.SlotTable.ActiveCount,
            "the finished escape leaves B8AC resident until room reload");
    }

    /// <summary>
    /// Reproduces the retail Fireflea/Colosseum defect pattern: a colored door can have a
    /// byte coordinate below the native allocation ceiling but outside the authored room.
    /// Native allocates the slot and runs setup against level_data anyway. The translation
    /// must preserve that harmless order without weakening bounds for PLMs whose setup
    /// genuinely consumes logical terrain.
    /// </summary>
    private static void VerifyNativeOutOfBoundsPopulationSetupOrder()
    {
        const ushort population = 0x96c0;
        const ushort malformedPopulation = 0x96e0;
        const int width = 16;
        const int height = 32;
        const int offRoomX = 1;
        const int offRoomY = 38;
        const int inRoomX = 2;
        const int inRoomY = 2;

        var decoded = new RoomObjectPopulation($"$8F:{population:X4}",
        [
            new RoomObjectPlacement(RoomObjectKinds.RequirePlaced(RoomObjectKind.GreenDoorFacingRight),
                new(offRoomX, offRoomY), new DoorBitArgument(SuperMetroid.AssetExtraction.Progression.RoomPlacementDecoding.Door(0x27), DoorOpeningPersistence.Persistent)),
            new RoomObjectPlacement(RoomObjectKinds.RequirePlaced(RoomObjectKind.GreenDoorFacingLeft),
                new(inRoomX, inRoomY), new DoorBitArgument(SuperMetroid.AssetExtraction.Progression.RoomPlacementDecoding.Door(0x28), DoorOpeningPersistence.Persistent)),
        ]);

        RoomLevelData level = CreateRoom(new(width, height),
            new RoomTerrainBlock[width * height],
            new RoomBlockBehavior[width * height]);
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertEqual(2, RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, new SharedGameState(new SharedRandom()), AreaId.Brinstar), decoded, new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "off-room colored door and following record both run setup");
        RoomObjectSlotSnapshot[] slots = roomObjects.PopulationSlots.ToArray();
        AssertEqual(0, slots[0].ProcessingPosition,
            "off-room record is allocated before its setup callback");
        AssertEqual(level.BlockAt(offRoomY * width + offRoomX), slots[0].Block,
            "off-room record sits on the block its column reaches in the following rows");
        AssertEqual(1, slots[1].ProcessingPosition,
            "following record is processed after it");
        AssertEqual(RoomTerrainBlock.Of(0x000, BlockMirroring.None, RoomCollisionType.Air), level.GetCollisionBlockByIndex(width * height - 1).Terrain,
            "off-room setup cannot corrupt the final authored host block");

        // Both resident actors execute their first draw. The off-room write remains in the
        // bounded native tail while the ordinary record updates logical terrain normally.
        roomObjects.Step(level);
        AssertEqual(RoomTerrainBlock.Of(0x004, BlockMirroring.None, RoomCollisionType.ShootableBlock), level.GetCollisionBlock(new(inRoomX, inRoomY)).Terrain,
            "in-room record following the harmless tail actor still draws normally");

        var malformed = new RoomObjectPopulation($"$8F:{malformedPopulation:X4}",
        [
            new RoomObjectPlacement(RoomObjectKinds.RequirePlaced(RoomObjectKind.ScrollTrigger),
                new(offRoomX, offRoomY), null, Array.Empty<RoomScrollWrite>()),
        ]);
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomObjectFixtures.LoadPopulation(RoomObjectFixtures.Unbound(), new RoomObjectRoom(level, new SharedGameState(new SharedRandom()), AreaId.Brinstar), malformed, new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "off-room scroll owner fails when setup consumes a required logical block");
    }

    /// <summary>
    /// Exercises both native uses of header $C8CA: an ordinary room load leaves its
    /// six-frame closed gate list intact, while Mother Brain's direction-nine entrance
    /// redirects that same physical slot to the header's second, three-frame closing list.
    /// </summary>
    internal static void VerifyMotherBrainEscapeRoomGate(TestAddressSpace bus)
    {
        const ushort population = 0x9680;
        const int width = 16;
        const int height = 16;
        const int gateX = 7;
        const int gateY = 2;
        RoomTerrainBlock originalGateWord = RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.BombableBlock);
        RoomTerrainBlock deactivatedGateWord = RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock);
        RoomTerrainBlock openGateWord = RoomTerrainBlock.Of(0x0ff, BlockMirroring.None, RoomCollisionType.SolidBlock);
        RoomTerrainBlock halfClosedGateTopWord = RoomTerrainBlock.Of(0x30f, BlockMirroring.None, RoomCollisionType.SolidBlock);
        RoomTerrainBlock closedGateMiddleWord = RoomTerrainBlock.Of(0x2e8, BlockMirroring.Vertical, RoomCollisionType.SolidBlock);

        SeedObjectProgramWords(bus, 0xc8ca, 0xb3c1);
        SeedObjectProgramWords(bus, 0xc8cc, RoomObjectAnimationScriptsTooling.MotherBrainEscapeRoomGateClosed);
        SeedObjectProgramWords(bus, 0xc8ce, RoomObjectAnimationScriptsConstants.MotherBrainEscapeRoomGateClosing);
        SeedObjectProgramWords(bus, 0xc8d0, 0xb3c1);
        SeedRoomObjectPopulation(bus, population,
        [
            0xca, 0xc8, gateX, gateY, 0x00, 0x80,
            0x00, 0x00,
        ]);

        SeedObjectProgramWords(bus, 0xbb34, 6);
        SeedObjectProgramWords(bus, 0xbb36, 0x948b);
        SeedObjectProgramWords(bus, 0xbb38, (ushort)RoomObjectInstruction.Delete);
        SeedObjectProgramWords(bus, 0xbb44, 2);
        SeedObjectProgramWords(bus, 0xbb46, 0x9473);
        SeedObjectProgramWords(bus, 0xbb48, 2);
        SeedObjectProgramWords(bus, 0xbb4a, 0x947f);
        SeedObjectProgramWords(bus, 0xbb4c, 2);
        SeedObjectProgramWords(bus, 0xbb4e, 0x948b);
        SeedObjectProgramWords(bus, 0xbb50, (ushort)RoomObjectInstruction.Delete);
        WriteVerticalObjectDraw(bus, 0x9473, [0x80ff, 0x80ff, 0x80ff, 0x80ff]);
        WriteVerticalObjectDraw(bus, 0x947f, [0x830f, 0x80ff, 0x80ff, 0x830f]);
        WriteVerticalObjectDraw(bus, 0x948b, [0x830f, 0x8ae8, 0x82e8, 0x830f]);
        var guarded = NewMotherBrainEscapeGateGuard(bus);

        RoomLevelData ordinaryLevel = CreateRoom(new(width, height),
            new RoomTerrainBlock[width * height],
            new RoomBlockBehavior[width * height]);
        int gateBlock = ordinaryLevel.GetBlockIndex(new(gateX, gateY));
        ordinaryLevel.SetTerrain(gateBlock, originalGateWord);
        var ordinary = RoomObjectFixtures.Unbound();
        var system = new SharedGameState(new SharedRandom());
        AssertEqual(1, RoomObjectFixtures.LoadPopulation(ref ordinary, new RoomObjectRoom(ordinaryLevel, system, AreaId.Tourian), RoomObjectPopulationImporter.Read(guarded, population), new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "escape gate population record is accepted by the sequential loader");
        AssertEqual(deactivatedGateWord, ordinaryLevel.GetCollisionBlockByIndex(gateBlock).Terrain,
            "C8CA setup clears collision class bits without deleting its resident slot");
        AssertEqual(0, ordinary.PopulationSlots.Single().ProcessingPosition,
            "escape gate takes the first slot in processing order");
        AssertEqual(DoorObjectNativeListAddresses.EscapeGateShut, TypedDoorObjectOracle.StartListOf(ordinary.PopulationSlots.Single().Behaviour),
            "ordinary entry places C8CA's gate shut, as its first list");
        ordinary.Step(ordinaryLevel);
        AssertEqual(closedGateMiddleWord,
            ordinaryLevel.GetCollisionBlock(new(gateX, gateY + 1)).Terrain,
            "non-escape entry draws the cartridge's already-closed gate");
        for (int frame = 0; frame < 6; frame++)
            ordinary.Step(ordinaryLevel);
        AssertEqual(0, ordinary.SlotTable.ActiveCount,
            "non-escape closed-gate actor releases its slot after six authored frames");

        RoomLevelData escapeLevel = CreateRoom(new(width, height),
            new RoomTerrainBlock[width * height],
            new RoomBlockBehavior[width * height]);
        escapeLevel.SetTerrain(gateBlock, originalGateWord);
        var escape = RoomObjectFixtures.Unbound();
        RoomObjectFixtures.LoadPopulation(ref escape, new RoomObjectRoom(escapeLevel, system, AreaId.Tourian), RoomObjectPopulationImporter.Read(guarded, population), new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
        var motherBrainExit = new DoorDefinition(
            Name: nameof(TourianDoorDefinitions.MotherBrain1),
            DestinationRoom: RoomDefinitions.TourianEscape1,
            Orientation: new(DoorDirection.Left, DoorClosingBehavior.EscapeGateCloses),
            RoomObjectBlock: new(gateX, gateY),
            DestinationScreen: new(0, 0),
            SamusSpeed: DoorSamusSpeed.AxisDefault,
            SetupCode: DoorSetupCode.None);
        AssertTrue(escape.DoorObjects.TrySpawnDoorClosingObject(escapeLevel, motherBrainExit, system),
            "direction-nine Mother Brain exit redirects the resident gate");
        AssertEqual(1, escape.SlotTable.ActiveCount,
            "resident escape gate is redirected in place rather than duplicated");
        AssertEqual(0, escape.PopulationSlots.Single().ProcessingPosition,
            "redirect preserves the room-population slot order");
        AssertTrue(TypedDoorObjectOracle.IsClosing(escape.PopulationSlots.Single().Behaviour),
            "door transition closes C8CA's gate, as its second list");

        escape.Step(escapeLevel);
        AssertEqual(openGateWord, escapeLevel.GetCollisionBlock(new(gateX, gateY)).Terrain,
            "closing frame one draws the fully open gate");
        escape.Step(escapeLevel);
        AssertEqual(openGateWord, escapeLevel.GetCollisionBlock(new(gateX, gateY)).Terrain,
            "two-frame gate timer retains the open image on its countdown frame");
        escape.Step(escapeLevel);
        AssertEqual(halfClosedGateTopWord, escapeLevel.GetCollisionBlock(new(gateX, gateY)).Terrain,
            "closing frame two draws the half-closed gate");
        escape.Step(escapeLevel);
        escape.Step(escapeLevel);
        AssertEqual(closedGateMiddleWord, escapeLevel.GetCollisionBlock(new(gateX, gateY + 1)).Terrain,
            "closing frame three installs the final solid gate collision");
        escape.Step(escapeLevel);
        escape.Step(escapeLevel);
        AssertEqual(0, escape.SlotTable.ActiveCount,
            "escape gate releases its slot after the final two-frame hold");

        RoomLevelData fallbackLevel = CreateRoom(new(width, height),
            new RoomTerrainBlock[width * height],
            new RoomBlockBehavior[width * height]);
        fallbackLevel.SetTerrain(gateBlock, originalGateWord);
        var fallback = RoomObjectFixtures.Unbound();
        AssertTrue(fallback.DoorObjects.TrySpawnDoorClosingObject(fallbackLevel, motherBrainExit, system),
            "special door without a resident cap spawns C8D0 fallback");
        AssertTrue(fallback.PopulationSlots.Single().Behaviour is MotherBrainEscapeGateObject { Change: var closing }
                && closing == MotherBrainEscapeGateDefinitions.Closing,
            "fallback actor is the escape gate closing behind Samus");
        AssertEqual(deactivatedGateWord, fallbackLevel.GetCollisionBlockByIndex(gateBlock).Terrain,
            "fallback C8D0 executes the shared deactivate setup");

        var nonClosingDoor = motherBrainExit with { Orientation = new(DoorDirection.Right, DoorClosingBehavior.None) };
        var nonClosing = RoomObjectFixtures.Unbound();
        AssertTrue(!nonClosing.DoorObjects.TrySpawnDoorClosingObject(fallbackLevel, nonClosingDoor, system),
            "directions zero through three retain the native no-closing-PLM branch");
        AssertEqual(0, nonClosing.SlotTable.ActiveCount,
            "non-closing door does not consume a PLM slot");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Mother Brain escape gate never rereads compiled programs or physical draws");
    }

    /// <summary>Compares the Mother Brain escape gate's compiled program, drawing and collision data with retail words.</summary>
    internal static void VerifyMotherBrainEscapeGateCompiledDefinitions(
        CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyEscapeGateChanges), () => VerifyEscapeGateChanges(rom));
        Suite(nameof(VerifyEscapeGateProgramControls), () => VerifyEscapeGateProgramControls(rom));
        Suite(nameof(VerifyEscapeGateProgramDraws), () => VerifyEscapeGateProgramDraws(rom));
        Suite(nameof(VerifyEscapeGateDrawGeometry), () => VerifyEscapeGateDrawGeometry(rom));
        Suite(nameof(VerifyEscapeGateDrawCollision), () => VerifyEscapeGateDrawCollision(rom));
    }

    /// <summary>The escape gate's shut and closing changes play as their native lists do (#1554).</summary>
    private static void VerifyEscapeGateChanges(CartridgeImportAddressSpace rom)
    {
        var cartridge = CartridgeImportSource.Require(rom);
        int frames = NativeTerrainChanges.Compare(cartridge, MotherBrainEscapeGateDefinitions.Shut, DoorObjectNativeListAddresses.EscapeGateShut,
            NativeTerrainChanges.NoSteps, "escape gate shut", out bool shutBegan);
        frames += NativeTerrainChanges.Compare(cartridge, MotherBrainEscapeGateDefinitions.Closing, DoorObjectNativeListAddresses.EscapeGateClosing,
            NativeTerrainChanges.NoSteps, "escape gate closing", out bool closingBegan);
        AssertTrue(!shutBegan && !closingBegan, "the escape gate opens nothing as it starts");
        Console.WriteLine($"  Escape gate: {frames} typed frames match bank $84.");
    }

    /// <summary>Writes a terminated vertical PLM draw stream with one level word per cell.</summary>
    private static void WriteVerticalObjectDraw(
        TestAddressSpace bus,
        ushort pointer,
        ReadOnlySpan<ushort> levelWords)
    {
        SeedObjectProgramWords(bus, pointer, unchecked((ushort)(0x8000 | levelWords.Length)));
        for (int index = 0; index < levelWords.Length; index++)
            SeedObjectProgramWords(bus, unchecked((ushort)(pointer + 2 + index * 2)), levelWords[index]);
        SeedObjectProgramWords(bus, unchecked((ushort)(pointer + 2 + levelWords.Length * 2)), 0);
    }

    /// <summary>
    /// Exercises all semantic branches in `$DB44`'s thirteen-entry room-argument table:
    /// no-op, below-quota, four event writes, persistent sleep, and malformed input.
    /// </summary>
    internal static void VerifyMetroidsClearedStateObject(TestAddressSpace bus)
    {
        const ushort population = 0x9500;
        SeedObjectProgramWords(bus, 0xdb46, RoomObjectNativeListAddresses.MetroidsClearedSleepList);
        SeedObjectProgramWords(bus, 0xdb42, 0xdead);
        SeedRoomObjectPopulation(bus, population,
        [
            0x44, 0xdb, 0x01, 0x01, 0x10, 0x00,
            0x44, 0xdb, 0x02, 0x01, 0x12, 0x00,
            0x44, 0xdb, 0x03, 0x01, 0x14, 0x00,
            0x44, 0xdb, 0x04, 0x01, 0x16, 0x00,
            0x44, 0xdb, 0x05, 0x01, 0x18, 0x00,
            0x00, 0x00,
        ]);

        RoomLevelData level = CreateRoom(new(8, 8),
            new RoomTerrainBlock[64],
            new RoomBlockBehavior[64]);
        var system = new SharedGameState(new SharedRandom());
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertEqual(5, RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, system, AreaId.Tourian), RoomObjectPopulationImporter.Read(bus, population), new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Events = new FixtureEventFlags { HasEvent = system.HasEvent, SetEvent = system.SetEvent } }),
            "Metroids-cleared population loads all argument families");

        roomObjects.Step(level, enemyDeaths: 2, enemyDeathQuota: 3);
        foreach (StoryEvent eventNumber in new[]
                 {
                     StoryEvent.FirstMetroidHallCleared,
                     StoryEvent.FirstMetroidShaftCleared,
                     StoryEvent.SecondMetroidHallCleared,
                     StoryEvent.SecondMetroidShaftCleared,
                 })
        {
            AssertTrue(!system.HasEvent(eventNumber),
                $"Metroids-cleared {eventNumber} remains clear below quota");
        }

        roomObjects.Step(level, enemyDeaths: 3, enemyDeathQuota: 3);
        foreach (StoryEvent eventNumber in new[]
                 {
                     StoryEvent.FirstMetroidHallCleared,
                     StoryEvent.FirstMetroidShaftCleared,
                     StoryEvent.SecondMetroidHallCleared,
                     StoryEvent.SecondMetroidShaftCleared,
                 })
        {
            AssertTrue(system.HasEvent(eventNumber),
                $"Metroids-cleared {eventNumber} is marked at quota");
        }
        AssertEqual(5, roomObjects.SlotTable.ActiveCount,
            "Metroids-cleared observers stay in the room after their quota is met");
        AssertTrue(roomObjects.PopulationSlots.OrderBy(slot => slot.ProcessingPosition).Select(slot => slot.Behaviour is MetroidRoomClearanceObject watch ? watch.Cleared : null).SequenceEqual(
                [null, StoryEvent.FirstMetroidHallCleared, StoryEvent.FirstMetroidShaftCleared, StoryEvent.SecondMetroidHallCleared, StoryEvent.SecondMetroidShaftCleared]),
            "Metroids-cleared observers are typed watches, each marking its entry's event");

        const ushort invalidPopulation = 0x9580;
        SeedRoomObjectPopulation(bus, invalidPopulation,
        [
            0x44, 0xdb, 0x01, 0x01, 0x01, 0x00,
            0x00, 0x00,
        ]);
        AssertThrows<InvalidDataException>(
            () => RoomObjectFixtures.LoadPopulation(RoomObjectFixtures.Unbound(), new RoomObjectRoom(level, new SharedGameState(new SharedRandom()), AreaId.Tourian), RoomObjectPopulationImporter.Read(bus, invalidPopulation), new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "odd Metroids-cleared room argument fails loudly");
    }

    /// <summary>
    /// The hardcoded room-setup spawn and actual PLM handler must install the
    /// compiled $B8D6 callback before the power-bomb and path-clear branches.
    /// </summary>
    private static void VerifyShaktoolRoomObject()
    {
        var bus = new TestAddressSpace();
        CartridgeImportAddressSpace? rom = File.Exists("Super Metroid.smc")
            ? SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc")
            : null;
        for (int index = 0; index < 3; index++)
        {
            ushort address = checked((ushort)(ShaktoolRoomObjectNativeProgram.AnimationScript + index * 2));
            AssertTrue(ShaktoolRoomObjectNativeProgram.TryReadInstructionWord(address, out ushort compiled),
                $"Shaktool room list word ${address:X4} is compiled");
            if (rom is not null)
            {
                int source = 0x840000 | address;
                AssertEqual(ReadRetailObjectProgramWord(rom, source),
                    compiled,
                    $"Shaktool room list word ${address:X4} matches pinned cartridge");
            }
            SeedObjectProgramWords(bus, address, 0xdead);
        }
        AssertTrue(!ShaktoolRoomObjectNativeProgram.TryReadInstructionWord(
                checked(ShaktoolRoomObjectNativeProgram.AnimationScript + 6), out _),
            "Shaktool room list owner excludes adjacent setup code");

        const ushort emptyPopulation = 0x9780;
        SeedRoomObjectPopulation(bus, emptyPopulation, [0x00, 0x00]);
        RoomLevelData level = CreateRoom(new(32, 32), new RoomTerrainBlock[32 * 32],
            new RoomBlockBehavior[32 * 32]);
        RoomScrollGrid scrolls = RoomScrollGrid.LoadNativeTable(
            new byte[RoomScrollGrid.KeptCellCount], new Extent16<RoomScreens>(2, 2));
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
        var system = new SharedGameState(new SharedRandom());
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertEqual(0, RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, system, AreaId.Maridia), RoomObjectPopulationImporter.Read(bus, emptyPopulation), new RoomObjectServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Events = new FixtureEventFlags { HasEvent = system.HasEvent, SetEvent = system.SetEvent } }),
            "empty fixture reserves no PLM before Shaktool setup spawn");
        AssertTrue(roomObjects.Shaktool.TrySpawnShaktoolRoomController(scrolls),
            "Shaktool room setup spawns its resident controller");
        AssertEqual(RoomScrollState.Blue, scrolls.ReadState(0),
            "Shaktool setup opens the first scroll cell");
        AssertEqual(RoomScrollState.RedBoundary, scrolls.ReadState(3),
            "Shaktool setup keeps later scroll cells red");

        roomObjects.Step(level, new RoomObjectFrameInputs { Scrolls = scrolls, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0 });
        RoomObjectSlotSnapshot sleeping = roomObjects.PopulationSlots.Single();
        AssertTrue(sleeping.Behaviour is ShaktoolRoomControllerObject { Watching: true },
            "after its first update the controller watches the room, as the native list's installed wait does");

        roomObjects.Step(level, new RoomObjectFrameInputs { Scrolls = scrolls, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0, PowerBombExplosionActive = true });
        for (int cell = 0; cell < ShaktoolRoomDefinitions.ScrollCellCount; cell++)
            AssertEqual(RoomScrollState.Blue, scrolls.ReadState(cell),
                $"Shaktool power bomb opens scroll cell {cell}");
        AssertEqual(1, roomObjects.SlotTable.ActiveCount,
            "Shaktool controller remains resident until Samus passes the threshold");

        samus.Position = samus.Position.WithWholeX(checked(ShaktoolRoomDefinitions.ClearedPathXBoundary + 1));
        roomObjects.Step(level, new RoomObjectFrameInputs { Scrolls = scrolls, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0 });
        AssertTrue(system.HasEvent(StoryEvent.ShaktoolClearedPath),
            "Shaktool path event follows the native X threshold");
        AssertEqual(0, roomObjects.SlotTable.ActiveCount,
            "Shaktool controller deletes itself when the path event is set");
    }

    /// <summary>Exercises map, missile and save-station admission and message timing in one populated room.</summary>
    internal static void VerifyOtherStationFamilies(TestAddressSpace bus)
    {
        const ushort population = 0x9200;
        SeedRoomObjectPopulation(bus, population, [
            0xd3, 0xb6, 0x06, 0x06, 0x00, 0x00,
            0xeb, 0xb6, 0x0c, 0x06, 0x00, 0x00,
            0x6f, 0xb7, 0x12, 0x06, 0x03, 0x00,
            0x00, 0x00,
        ]);
        const int width = 32;
        RoomLevelData level = CreateRoom(new(width, 16),
            new RoomTerrainBlock[width * 16],
            new RoomBlockBehavior[width * 16]);
        var system = new SharedGameState(new SharedRandom());
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Health = 99, MaxHealth = 99, Missiles = 0, MaxMissiles = 10 };
        var roomObjects = RoomObjectFixtures.Unbound();
        RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, system, AreaId.Norfair), RoomObjectPopulationImporter.Read(bus, population, AreaId.Norfair), new RoomObjectServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });

        AssertEqual(3, roomObjects.StationSnapshots.Count, "map, missile, and save parents share one pool");
        AssertEqual(0x47, RoomBlockBehaviorCodec.Encode(level.GetCollisionBlock(new(7, 6)).Behavior),
            "map right access setup uses BTS $47");
        AssertEqual(0x48, RoomBlockBehaviorCodec.Encode(level.GetCollisionBlock(new(4, 6)).Behavior),
            "map left access setup uses native two-block offset");
        AssertEqual(0x4b, RoomBlockBehaviorCodec.Encode(level.GetCollisionBlock(new(13, 6)).Behavior),
            "missile right access setup uses BTS $4B");
        AssertEqual(0x4c, RoomBlockBehaviorCodec.Encode(level.GetCollisionBlock(new(11, 6)).Behavior),
            "missile left access setup uses BTS $4C");
        AssertEqual(0x4d, RoomBlockBehaviorCodec.Encode(level.GetCollisionBlock(new(18, 6)).Behavior),
            "save station setup installs trigger BTS $4D");

        AssertTrue(roomObjects.TryNotifyStationTouch(level, new(7, 6), 0x47),
            "map access resolves parent");
        AssertTrue(samus.Control.AlphaLocked,
            "map access setup locks Samus before the resident PLM advances");
        AssertTrue(roomObjects.TryNotifyStationTouch(level, new(13, 6), 0x4b),
            "missile access resolves parent");
        // Activation runs on the triggering pass plus 6 + $60 later passes.
        for (int frame = 0; frame < 102; frame++)
            roomObjects.Step(level);
        AssertTrue(!system.HasAreaMap(2), "map station has not activated before its 103rd pass");
        roomObjects.Step(level);
        AssertEqual(RoomTerrainBlock.Of(0x059, BlockMirroring.Vertical, RoomCollisionType.SpecialBlock), level.GetCollisionBlock(new(18, 6)).Terrain,
            "compiled save-pod idle draw installs its physical floor word");
        AssertEqual(RoomTerrainBlock.Of(0x05b, BlockMirroring.None, RoomCollisionType.Air), level.GetCollisionBlock(new(18, 2)).Terrain,
            "compiled save-pod draw extends its shaft above the trigger");
        AssertEqual(RoomTerrainBlock.Of(0x059, BlockMirroring.None, RoomCollisionType.SolidBlock), level.GetCollisionBlock(new(18, 1)).Terrain,
            "compiled save-pod draw installs its upper cap");
        AssertTrue(system.HasAreaMap(2), "map station marks current area acquired");
        AssertEqual(10, samus.Missiles, "missile station restores missiles");
        AssertEqual(2, roomObjects.Stations.StationActivationEvents.Count,
            "two station parents publish two explicit activations");
        // $84:8CE7 runs Samus command one as soon as the missile message returns; the map
        // activation never unlocks, but both fixtures share this Samus owner.
        AssertTrue(!samus.Control.AlphaLocked,
            "missile activation releases Samus after its completion message");

        // The runtime freezes this PLM while the message is open. Once bank $85 returns,
        // the original instruction lists execute three six-frame phases: post-message
        // hold, retract, and final hold, without looping the access sound.
        for (int frame = 0; frame < 18; frame++)
            roomObjects.Step(level);
        AssertEqual(RoomTerrainBlock.Of(0x128, BlockMirroring.None, RoomCollisionType.SolidBlock), level.GetCollisionBlock(new(7, 6)).Terrain,
            "map access retracts to its compiled right-side parent word");
        AssertEqual(RoomTerrainBlock.Of(0x128, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock), level.GetCollisionBlock(new(4, 6)).Terrain,
            "map access retracts its linked left-side block");
        AssertEqual(RoomTerrainBlock.Of(0x0c3, BlockMirroring.Horizontal, RoomCollisionType.SpecialBlock), level.GetCollisionBlock(new(13, 6)).Terrain,
            "missile access retracts to its compiled resource word");
        AssertTrue(!samus.Control.AlphaLocked,
            "Samus stays released through the post-message retraction");

        AssertTrue(roomObjects.TryNotifyStationTouch(level, new(18, 6), 0x4d),
            "save trigger resolves its same-block parent");
        roomObjects.Step(level);
        AssertEqual(StationKind.Save, roomObjects.Stations.StationActivationEvents.Single().Kind,
            "save trigger publishes typed confirmation request");
        AssertTrue(ReferenceEquals(SuperMetroid.Core.Progression.LoadStationDefinitions.CrocomireSave, roomObjects.Stations.StationActivationEvents.Single().Station),
            "save request carries the load station its room argument names");

        StationActivationEvent saveRequest = roomObjects.Stations.StationActivationEvents.Single();
        samus.Position = samus.Position.WithWholeX(0x0127);
        RoomTerrainBlock[] levelBeforeSave = Enumerable.Range(0, level.Size.X * level.Size.Y)
            .Select(block => level.GetCollisionBlock(new(checked((ushort)(block % level.Size.X)), checked((ushort)(block / level.Size.X)))).Terrain)
            .ToArray();
        bool saving = roomObjects.Stations.ResolveSaveStationConfirmation(saveRequest, accepted: true, level: level);
        AssertTrue(saving, "accepted save resumes the sleeping cartridge PLM");
        // #1269: $84:AFF4-$AFFA run in the PLM pass the message returned into. That pass resumes at the message-close
        // boundary, and the pass queues the sound as it runs (#1390, #1620).
        AssertEqual(new SoundRequest(SamusSound.Saving, SoundRequestPriority.Normal),
            roomObjects.Sounds.SoundRequests[^1], "the confirming pass queues cartridge library-one sound $2E");
        AssertTrue(Enumerable.Range(0, levelBeforeSave.Length).Any(block => levelBeforeSave[block] !=
                level.GetCollisionBlock(new(checked((ushort)(block % level.Size.X)), checked((ushort)(block / level.Size.X)))).Terrain),
            "the confirming pass draws the first electricity frame");
        AssertEqual(0x0120, samus.Position.X.Whole,
            "save animation centers Samus on the station's 16-pixel boundary");
        AssertTrue(SamusPoseMetadata.IsForwardFacingPose(samus.Pose),
            "save animation applies shared MakeSamusFaceForward setup");
        AssertTrue(samus.Control.AlphaLocked, "save animation owns Samus input");
        AssertEqual(level.GridBlock(new(18, 6)), saveRequest.Block,
            "save activation retains the executing PLM block for bank-$86 electricity");
        Suite(nameof(VerifySaveStationElectricity), () => VerifySaveStationElectricity(bus, level, saveRequest.Block));

        StationActivationEvent completion = default;
        bool completionPublished = false;
        int completionFrame = -1;
        int maximumSaveAnimationFrames =
            SaveStationAnimationDefinitions.SaveAnimationLoops * 8 + 8;
        for (int frame = 0; frame < maximumSaveAnimationFrames && !completionPublished; frame++)
        {
            roomObjects.Step(level);
            if (roomObjects.Stations.StationActivationEvents.Count != 0)
            {
                completion = roomObjects.Stations.StationActivationEvents.Single();
                completionPublished = true;
                completionFrame = frame;
            }
        }
        AssertTrue(completionPublished, "save animation reaches completion message");
        // The 21st loop's second frame holds its four frames; $84:B002 then falls through
        // to the message 168 frames after the confirming pass, as in the 100% movie.
        AssertEqual(SaveStationAnimationDefinitions.SaveAnimationLoops * 8 - 1,
            completionFrame,
            "save animation publishes completion after all 21 alternating native loops");
        AssertEqual(GameplayMessageIds.SaveCompleted, completion.MessageBoxIndex,
            "save animation publishes game-saved message only after its draw loop");
        roomObjects.Stations.CompleteSaveStation(completion);
        AssertTrue(!samus.Control.AlphaLocked, "game-saved message return unlocks Samus");
        AssertTrue(roomObjects.StationSnapshots.Single(station => station.Kind == StationKind.Save)
            .SaveStationLockedOut,
            "save completion installs once-per-room-entry lockout");

        AssertTrue(roomObjects.TryNotifyStationTouch(level, new(18, 6), 0x4d),
            "locked save trigger remains a solid station block");
        roomObjects.Step(level);
        AssertEqual(0, roomObjects.Stations.StationActivationEvents.Count,
            "locked save station cannot reopen until room re-entry");
    }

    /// <summary>Checks the save station's enemy-projectile electricity art and placement at its room block.</summary>
    private static void VerifySaveStationElectricity(
        TestAddressSpace bus,
        RoomLevelData level,
        Position16<RoomBlocks> saveBlock)
    {
        // The native control stream is compiled. Constructed artwork is supplied by
        // operand identity, rather than shortening a bank-$86 program on a fake bus.
        SeedEmptyRoomEnemyPopulation(bus, 0x9600);
        SeedSaveStationElectricityProjectile(bus);

        var visible = new SpriteVisualPart
        {
            OffsetX = 0, OffsetY = 0, TileColumn = 1, TileRow = 0,
            Size = 8, Priority = 1, Palette = 0, FlipX = false, FlipY = false,
        };
        HashSet<EnemyProjectileFrameId> electricityFrames = [.. SaveStationElectricityAnimations.Segments.Frames];
        var artworkDocument = new EnemyProjectileSpritemapDocument
        {
            Version = EnemyProjectileSpritemapDocuments.Version,
            Frames = EnemyProjectileSpritemapDefinitions.Frames.ToDictionary(frame => frame.Name, _ => Array.Empty<SpriteVisualPart>()),
            ProgramFrames = EnemyProjectilePresentationFrameSets.All.ToDictionary(
                frame => frame.Name, frame => electricityFrames.Contains(frame.Frame) ? new[] { visible } : Array.Empty<SpriteVisualPart>()),
        };
        EnemyTileSourceArtwork tiles = EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(),
            // The save room's sprite objects compose with the installed enemy compositions, and their sheet takes the
            // n00b tube's crack shade every spawned-object sheet carries (#1512).
            Spritemaps = InstalledFixtures.EnemyTiles.Spritemaps, ChozoAndTubeColors = InstalledFixtures.EnemyTiles.ChozoAndTubeColors,
            ProjectileSpritemaps = EnemyProjectileSpritemapDocuments.Load(new MemoryStream(
                EnemyProjectileSpritemapDocuments.Write(artworkDocument))) });

        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledFrames(tiles), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: bus, services: new StandaloneRoomEnemyServices(() => 0x4040));
        enemies.Load(new RoomEnemyLoadRequest(bus.ReadEnemyPopulation(0x9600), BgArtworkFixtures.For(new SnesVram())) { Level = level, Samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) });
        // The fixture room places no enemies, so loading it keeps the session's first set; a save room's own set holds
        // the electricity on its shared sheet, as entering a save room loads it (#1513).
        SpawnedObjectFixtures.LoadSetSpawning(enemies, EnemyProjectileFrameId.SaveStationElectricityStartFirst,
            SpriteGraphics.SharedIn(SuperMetroid.Core.Enemies.Drawing.EnemyShades.InProjectileLook));
        enemies.Projectiles.SpawnSaveStationElectricity(saveBlock);

        EnemyProjectile electricity = enemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.SaveStationElectricity);
        AssertEqual(0x0130, electricity.Position.X.Whole,
            "save electricity starts one block right of the station PLM");
        AssertEqual(0x0040, electricity.Position.Y.Whole,
            "save electricity starts two blocks above the station PLM");

        enemies.StepEnemyProjectiles(level, samus: null);
        AssertEqual(EnemyProjectileProgramFrameSources.Operand(EnemyProjectileFrameId.SaveStationElectricityStartFirst),
            electricity.PresentationOperandAddress, "save electricity selects its first installed frame through its typed program");
        AssertEqual(EnemyProjectileSpritemapDefinitions.BlankSpritemap, electricity.SpritemapPointer,
            "installed program artwork does not retain a cartridge spritemap pointer");
        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawEnemyProjectiles(oam, new(0x0100, 0));
        oam.FinalizeFrame();
        AssertTrue(Enumerable.Range(0, oam.LastFinalizedSpriteCount)
            .Select(oam.GetEntry)
            .Any(entry => entry.Position.X == 0x0030 && entry.Position.Y == 0x40),
            "save electricity reaches the ordinary room enemy-projectile draw path");
    }

    /// <summary>
    /// Checks that an unlabelled resident header word reports its room pointer, record, block and argument. RoomObjectKind's
    /// values are arbitrary (#1414), so such a word is rejected by the import instead of reaching the loader; the check
    /// now asserts the import's message.
    /// </summary>
    private static void VerifyUnsupportedPopulationContext(TestAddressSpace bus)
    {
        const ushort population = 0x9400;
        const ushort unsupportedHeader = 0xb123;
        SeedObjectProgramWords(bus, unsupportedHeader, 0x9876);
        SeedObjectProgramWords(bus, unchecked(unsupportedHeader + 2), 0x9abc);
        SeedRoomObjectPopulation(bus, population, [
            0x23, 0xb1, 0x03, 0x04, 0x55, 0xaa, 0x00, 0x00,
        ]);
        InvalidDataException? error = null;
        try
        {
            RoomObjectPopulationImporter.Read(bus, population);
        }
        catch (InvalidDataException caught)
        {
            error = caught;
        }
        AssertTrue(error is not null,
            "unlabelled room-population header fails at import");
        AssertTrue((error ?? throw new InvalidOperationException("Verification requires error.")).Message.Contains("$8F:9400", StringComparison.Ordinal) &&
            error.Message.Contains("record 0", StringComparison.Ordinal) &&
            error.Message.Contains("$84:B123", StringComparison.Ordinal) &&
            error.Message.Contains("(3,4)", StringComparison.Ordinal) &&
            error.Message.Contains("$AA55", StringComparison.Ordinal),
            "unsupported PLM failure includes population/header/record/coordinate/argument context");
    }

    /// <summary>Checks the native save-confirmation message's delayed controller reads and default acceptance.</summary>
    private static void VerifySaveStationConfirmation(TestAddressSpace bus)
    {
        BackgroundTile[] Cells(int count, ushort first) => [.. Enumerable.Range(0, count)
            .Select(index => BackgroundTileWordBytes.Tile(first + index))];
        var notices = new GameplayMessageNoticeDocument
        {
            Version = GameplayMessageNoticeDefinitions.Version,
            Border = Cells(SuperMetroid.AssetExtraction.Hud.GameplayMessageTilemapDefinitions.TilemapWidth, 0x3801),
            Notices = GameplayMessageNoticeDefinitions.MessageIds.ToDictionary(id => id.ToString(), id =>
                new GameplayMessageNotice
                {
                    RowCount = GameplayMessageNotices.ContentRows(id),
                    Template = Cells(GameplayMessageNotices.ContentRows(id) * SuperMetroid.AssetExtraction.Hud.GameplayMessageTilemapDefinitions.TilemapWidth, 0x2800),
                    Text = [new GameplayMessageTextRegion { Row = 0, Column = 0, Width = 4,
                        Alignment = GameplayMessageTextAlignment.Left, Text = "TEST", Palette = 0 }],
                    YesSelection = GameplayMessageNotices.IsSaveConfirmation(id) ? Cells(32, 0x5100) : null,
                    NoSelection = GameplayMessageNotices.IsSaveConfirmation(id) ? Cells(32, 0x5200) : null,
                }),
        };
        using var noticeJson = new MemoryStream();
        GameplayMessageNoticePresentation.Write(noticeJson, notices);
        noticeJson.Position = 0;
        GameplayMessageNoticePresentation presentation = GameplayMessageNoticePresentation.Load(noticeJson);
        SeedSaveConfirmationMessageCartridge(bus);

        var messages = new GameplayMessageArtwork(null, notices: presentation);
        var message = new GameplayMessageBoxState(new SaveChoiceInputCadence());
        message.Begin(GameplayMessageIds.SaveConfirmation, 0);
        for (int guard = 0;
             message.Phase != GameplayMessageBoxPhase.AwaitingInput && guard < 32;
             guard++)
        {
            message.Step(0);
        }
        AssertEqual(GameplayMessageBoxPhase.AwaitingInput, message.Phase,
            "save message enters shared yes/no selection phase");
        AssertEqual(BackgroundTileWords.ToTile(0x5100), message.Tilemap(messages)[128],
            "save message begins with native selected-YES row");
        // $85:84BA waits two lag frames before its first ReadControllerInput.
        message.Step(0);
        message.Step(ControllerButton.Left);
        AssertTrue(message.ConfirmationSelectionYes, "save selector does not read during its lag waits");
        message.Step(ControllerButton.Left);
        AssertTrue(!message.ConfirmationSelectionYes,
            "save cursor changes the shared confirmation selection");
        AssertEqual(BackgroundTileWords.ToTile(0x5200), message.Tilemap(messages)[128],
            "save cursor redraws the native selected-NO row");
        // Later reads come every second frame: the read's frame holds the next first wait.
        // A toggle's redraw wait precedes that cycle once (#1269), so the first read after
        // it comes three frames later.
        message.Step(ControllerButton.Left);
        message.Step(ControllerButton.Left);
        message.Step(ControllerButton.Left);
        AssertTrue(!message.ConfirmationSelectionYes,
            "holding a direction does not toggle the newly-pressed save cursor repeatedly");
        message.Step(ControllerButton.A);
        AssertEqual(GameplayMessageBoxPhase.AwaitingInput, message.Phase, "A is read on the next read frame");
        message.Step(ControllerButton.A);
        AssertEqual(GameplayMessageBoxPhase.Closing, message.Phase, "A confirms on a read frame");
        for (int guard = 0; message.IsActive && guard < 32; guard++)
            message.Step(0);
        AssertEqual(false, message.ConsumeConfirmationResult(),
            "save confirmation publishes selected no result after close");

        message.Begin(GameplayMessageIds.SaveConfirmation, 0);
        for (int guard = 0;
             message.Phase != GameplayMessageBoxPhase.AwaitingInput && guard < 32;
             guard++)
        {
            message.Step(0);
        }
        message.Step(0);
        message.Step(0);
        message.Step(ControllerButton.A);
        for (int guard = 0; message.IsActive && guard < 32; guard++)
            message.Step(0);
        AssertEqual(true, message.ConsumeConfirmationResult(),
            "save confirmation defaults to yes and publishes acceptance");
    }

    /// <summary>Seeds the sparse bank-$84 header pointers and elevator frame loop needed by population fixtures.</summary>
    internal static void SeedRoomObjectPopulationRom(TestAddressSpace bus)
    {
        SeedPoseOneSamusData(bus);
        // Header instruction-list fields consumed by the generic allocator.
        SeedObjectProgramWords(bus, 0xb705, 0xaf86);
        SeedObjectProgramWords(bus, 0xb63d, 0xafa4);
        SeedObjectProgramWords(bus, 0xb70d, 0xafb6);
        SeedObjectProgramWords(bus, 0xb6e1, 0xadc2);
        SeedObjectProgramWords(bus, 0xb6d5, 0xad62);
        SeedObjectProgramWords(bus, 0xb6ed, 0xae4c);
        SeedObjectProgramWords(bus, 0xb771, 0xafe8);
        // Elevator's four synthetic frames. Station timing, pointer selection, and
        // draw payloads are compiled, so this sparse bus carries none of those bytes.
        SeedTimedDrawLoop(bus, 0xafb6, 4, 4, 0xa100);
        SeedObjectProgramWords(bus, 0xafc6, 0x8724);
        SeedObjectProgramWords(bus, 0xafc8, 0xafb6);
    }

    /// <summary>Writes a fixed-duration PLM draw sequence with a distinct one-block map for each frame.</summary>
    private static void SeedTimedDrawLoop(
        TestAddressSpace bus,
        ushort list,
        ushort timer,
        int frameCount,
        ushort firstDraw)
    {
        for (int frame = 0; frame < frameCount; frame++)
        {
            ushort draw = unchecked((ushort)(firstDraw + frame * 6));
            SeedObjectProgramWords(bus, unchecked((ushort)(list + frame * 4)), timer);
            SeedObjectProgramWords(bus, unchecked((ushort)(list + frame * 4 + 2)), draw);
            WriteOneBlockDraw(bus, draw, unchecked((ushort)(0x8000 | frame)));
        }
    }

    /// <summary>Rejects runtime rereads of compiled Mother Brain gate control and draw bytes.</summary>
    internal static CartridgeReadGuard NewMotherBrainEscapeGateGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(
            address => address is >= 0x84bb34 and <= 0x84bb51 or >= 0x849473 and <= 0x849496,
            address => $"Mother Brain escape gate reread compiled source ${address:X6}.");

    /// <summary>The dormant scroll fixture owns an empty terminated bank-$8F scroll program at $9100.</summary>
    private static void SeedDormantScrollObjectProgram(TestAddressSpace bus)
    {
        bus.WriteBytes(0x8f9100, [0x80]); // Dormant scroll fixture owns an empty terminated program.
    }

    /// <summary>Seeds the save-station electricity enemy-projectile definition, animation list and frame map.</summary>
    private static void SeedSaveStationElectricityProjectile(TestAddressSpace bus)
    {
        bus.WriteBytes(0x86e6d2,
        [
            0xad, 0xe6,
            0xd1, 0xe6,
            0x83, 0xe6,
            0x00, 0x00,
            0x00, 0x30,
            0x00, 0x00,
            0xfc, 0x84,
        ]);
        bus.WriteBytes(0x86e683,
        [
            0xd5, 0x81, 0x14, 0x00,
            0x01, 0x00, 0x62, 0xb5,
            0x54, 0x81,
        ]);
        // Enemy-projectile animation lists live in bank $86, but their frame maps are
        // consumed by the shared bank-$8D spritemap writer.
        bus.WriteBytes(0x8db562,
        [
            0x01, 0x00,
            0x00, 0x00, 0x00, 0x01, 0x20,
        ]);
    }

    /// <summary>Seeds message $17's bank-$85 definition record and the tilemaps it names.</summary>
    private static void SeedSaveConfirmationMessageCartridge(TestAddressSpace bus)
    {
        const int definitions = 0x85869b;
        int message23 = definitions + (0x17 - 1) * 6;
        WriteTestWord(bus, message23, 0x8436);
        WriteTestWord(bus, message23 + 2, 0x8289);
        WriteTestWord(bus, message23 + 4, 0x9800);
        WriteTestWord(bus, message23 + 6, 0x8436);
        WriteTestWord(bus, message23 + 8, 0x8289);
        WriteTestWord(bus, message23 + 10, 0x9900);
        WriteTestWord(bus, message23 + 12, 0x8436);
        WriteTestWord(bus, message23 + 14, 0x8289);
        WriteTestWord(bus, message23 + 16, 0x9940);
        for (int word = 0; word < 32; word++)
        {
            WriteTestWord(bus, 0x858040 + word * 2, 0x3801);
            WriteTestWord(bus, 0x859581 + (32 + word) * 2,
                unchecked((ushort)(0x5100 + word)));
            WriteTestWord(bus, 0x859581 + (64 + word) * 2,
                unchecked((ushort)(0x5200 + word)));
        }
        for (int word = 0; word < 128; word++)
            WriteTestWord(bus, 0x859800 + word * 2, unchecked((ushort)(0x2800 + word)));
        for (int word = 0; word < 32; word++)
            WriteTestWord(bus, 0x859900 + word * 2, unchecked((ushort)(0x2a00 + word)));
    }

    /// <summary>Runs room population, station and escape-gate PLM checks through the command dispatcher.</summary>
    [VerificationCommand("--room-plm-population")]
    internal static int RoomObjectPopulationCommand()
    {
     VerifySequentialRoomObjectPopulationLoader(); return 0; 
    }

    /// <summary>Runs retail door-closing definition checks through the command dispatcher.</summary>
    [VerificationCommand("--door-closing-definitions")]
    internal static int DoorClosingDefinitionsCommand()
    {
        Suite(nameof(VerifyDoorClosingObjectDefinitions), () => VerifyDoorClosingObjectDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
