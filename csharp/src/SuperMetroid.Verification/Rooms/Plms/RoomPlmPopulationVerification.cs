using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.Plms.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.Plms.CollectiblesVerification;
using static SuperMetroid.Verification.Areas.Escape.EscapeGateVisualsVerification;
using static SuperMetroid.Verification.Enemies.Elevator.LookupElevatorPlatformLayoutsVerification;
using static SuperMetroid.Verification.Enemies.Elevator.LookupElevatorPlatformProgramVerification;
using static SuperMetroid.Verification.Areas.Escape.LookupEscapeGateVerification;
using static SuperMetroid.Verification.Rooms.Plms.LookupSpeedEscapeStagesVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyFullMovieVerification;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
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
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Verification.Rooms.Doors;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;

/// <summary>Checks native PLM population order, resident doors, stations and escape-room controllers.</summary>
internal static class RoomPlmPopulationVerification
{
    /// <summary>Compares the elevator control list, draw selection and collision layouts against the retail cartridge.</summary>
    internal static void VerifyElevatorPlatformPlmDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyElevatorPlatformControls), () => VerifyElevatorPlatformControls(rom));
        Suite(nameof(VerifyElevatorPlatformDrawSelection), () => VerifyElevatorPlatformDrawSelection(rom));
        Suite(nameof(VerifyElevatorPlatformLoopTarget), () => VerifyElevatorPlatformLoopTarget(rom));

        Suite(nameof(VerifyElevatorPlatformLayoutGeometry), () => VerifyElevatorPlatformLayoutGeometry(rom));
        Suite(nameof(VerifyElevatorPlatformLayoutCollision), () => VerifyElevatorPlatformLayoutCollision(rom));
        Suite(nameof(VerifyElevatorPlatformLayoutVisuals), () => VerifyElevatorPlatformLayoutVisuals(rom));
        Console.WriteLine("Elevator platform PLM: native instruction loop and three complete draw lists match cartridge.");
    }

    /// <summary>Compares every closing-door orientation's compiled header against the bank-$8F table.</summary>
    private static void VerifyFallbackDoorHeaders(SuperMetroidAddressSpace rom) => VerifyFallbackDoorField(rom, false);
    /// <summary>Compares the selected resident door's initial list with its bank-$84 header word.</summary>
    private static void VerifyFallbackDoorLists(SuperMetroidAddressSpace rom) => VerifyFallbackDoorField(rom, true);

    /// <summary>Entries in <c>Door_Closing_PLMs</c> ($8F:E68A), the orientation byte's domain.</summary>
    private const byte DoorClosingTableEntries = 12;

    /// <summary>Checks all orientation bytes, including out-of-table rejection, against either headers or first-list words.</summary>
    private static void VerifyFallbackDoorField(SuperMetroidAddressSpace rom, bool list)
    {
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            byte direction = (byte)raw;
            if (raw >= DoorClosingTableEntries)
            {
                AssertThrows<InvalidDataException>(() => CartridgeDoorOrientation.Decode(direction), "Orientation beyond Door_Closing_PLMs rejected");
                if (!list) AssertThrows<InvalidDataException>(() => DoorClosingPlmRomData.GetHeader(direction), "Fallback door public rejected direction");
                continue;
            }
            ushort header = ReadSamusEaterPlmWord(rom, 0x8fe68a + raw * 2);
            CartridgeDoorOrientation orientation = CartridgeDoorOrientation.Decode(direction);
            AssertEqual(direction, orientation.Encode(), "Orientation byte round-trips");
            var actual = DoorClosingPlmRomData.GetDefinition(orientation);
            AssertEqual(header == 0, actual is null, "Non-closing table entries select no actor");
            if (list)
                AssertEqual(header == 0 ? (ushort)0 : ReadSamusEaterPlmWord(rom, 0x840000 | (header + 2)),
                    actual is { } fallback ? PlmProgramStartOracle.NativeListOf(fallback.Start) : (ushort)0,
                    "Fallback original initial list or no-actor zero");
            else
            {
                AssertEqual(header, (ushort)(actual?.Header ?? PlmHeaderId.None), "Fallback original direction header");
                AssertEqual(header, DoorClosingPlmRomData.GetHeader(direction), "Fallback public original header");
            }
        }
    }

    /// <summary>Checks compiled door-closing definitions and their admission through actual door PLM allocation.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for door closing PLM definitions")]
    internal static void VerifyDoorClosingPlmDefinitions(SuperMetroidAddressSpace rom, bool fallbackOnly = false)
    {

        Suite(nameof(VerifyFallbackDoorHeaders), () => VerifyFallbackDoorHeaders(rom));
        Suite(nameof(VerifyFallbackDoorLists), () => VerifyFallbackDoorLists(rom));
        for (byte direction = 0; direction < DoorClosingTableEntries; direction++)
        {
            ushort expectedHeader = CartridgeImportSource.Require(rom).ReadCartridgeWord((DoorClosingPlmRomData.HeaderTableAddress + direction * sizeof(ushort)));
            ushort expectedList = expectedHeader == 0
                ? (ushort)0
                : CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | unchecked((ushort)(expectedHeader + 2))));
            var plms = new RoomPlmSystem();
            RoomLevelData level = CreateRoom(new(4, 4),
                new ushort[16],
                new byte[16],
                blockDefinitions: new byte[0x400 * 8]);
            var door = new CartridgeDoorHeader(
                Pointer: 0,
                DestinationRoomPointer: 0,
                Orientation: CartridgeDoorOrientation.Decode(direction),
                PlmBlock: new(1, 1),
                DestinationScreen: new(0, 0),
                SamusDistance: 0,
                SetupCodePointer: DoorSetupCode.None);
            bool spawned = plms.DoorPlms.TrySpawnDoorClosingPlm(
                level, door, new Bank80SystemState());
            AssertEqual(expectedHeader != 0, spawned,
                $"door-closing direction {direction} fallback admission matches cartridge");
            if (expectedHeader == 0)
                continue;
            RoomPlmSlotSnapshot slot = plms.PopulationSlots.Single();
            AssertEqual(expectedHeader, (ushort)slot.HeaderPointer,
                $"door-closing direction {direction} installs compiled header");
            AssertTrue(PlmProgramStartOracle.IsRunning(PlmProgramStartOracle.ForNativeList(expectedList), slot),
                $"door-closing direction {direction} starts the program of its compiled initial list");
        }

        AssertThrows<InvalidDataException>(
            () => CartridgeDoorOrientation.Decode(DoorClosingTableEntries),
            "out-of-range door orientation fails loudly");
        if (fallbackOnly) return;
        Suite(nameof(VerifyResidentDoorClosingDefinitions), () => VerifyResidentDoorClosingDefinitions(rom));
        Suite(nameof(VerifyMotherBrainEscapeGateCompiledDefinitions), () => VerifyMotherBrainEscapeGateCompiledDefinitions(rom));
        Suite(nameof(VerifyEscapeGateVisuals), () => VerifyEscapeGateVisuals(rom));
        Suite(nameof(VerifySequentialRoomPlmPopulationLoader), () => VerifySequentialRoomPlmPopulationLoader());
        Console.WriteLine(
            "Door-closing definitions: all twelve fallback and eighteen resident selections match.");
    }

    /// <summary>
    /// Verifies the fixed header+4 closing list of every resident door: a typed cap's closing program begins exactly
    /// there, and the escape gate and eye doors resolve their closing start by identity. Each then runs the real
    /// room-population load and transition redirect with that source word absent from the sparse bus.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for resident door closing definitions")]
    internal static void VerifyResidentDoorClosingDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort[] headers = [0xbaf4,0xc842,0xc848,0xc84e,0xc854,0xc85a,0xc860,0xc866,0xc86c,
            0xc872,0xc878,0xc87e,0xc884,0xc88a,0xc890,0xc896,0xc89c,0xc8ca,0xdb4c,0xdb5a];
        PlmHeaderId[] resolved = [.. ResidentDoorClosingDefinitions.All.Select(entry => entry.Header)];
        PlmHeaderId[] identityClosers = [PlmHeaderId.MotherBrainEscapeRoomGate, PlmHeaderId.EyeDoorFacingRight, PlmHeaderId.EyeDoorFacingLeft];
        AssertTrue(resolved.SequenceEqual(identityClosers),
            "Only the escape gate and the eye doors keep a closing start by identity");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (!resolved.Contains((PlmHeaderId)raw))
                AssertThrows<InvalidDataException>(() => ResidentDoorClosingDefinitions.Resolve((PlmHeaderId)raw),
                    "Resident closing complete unsupported-header domain");

        int definitionIndex = 0;
        foreach (ushort raw in headers)
        {
            PlmHeaderId header = (PlmHeaderId)raw;
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | unchecked((ushort)(raw + 4))));
            ushort? typedClosing = DoorPlmNativeListAddresses.TypedClosingEntry(header);
            if (typedClosing is { } typed)
                AssertEqual(expected, typed, $"resident door $84:{raw:X4} closing program begins at its header's list");
            else
                AssertEqual(PlmProgramStartOracle.ForNativeList(expected), ResidentDoorClosingDefinitions.Resolve(header),
                    $"resident door $84:{raw:X4} resolves by identity");

            // Typed placement metadata supplies the first list only. The transition
            // must obtain its closing program from the resident cap or catalog,
            // not from an arbitrary address space or a header+4 fixture byte.
            var bus = new TestAddressSpace();
            const ushort population = 0x9000;
            byte blockX = (byte)(1 + definitionIndex % 4);
            byte blockY = (byte)(1 + definitionIndex / 4);
            var decoded = new RoomPlmPopulationDefinition(population,
            [
                // Two directional caps are not placed in any retail room population,
                // so they are absent from the 70-header population catalog. Decode
                // their setup/first-list metadata here, in the import-side oracle.
                new RoomPlmPlacement(new RoomPlmHeaderDefinition(header,
                    CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | raw)),
                    CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | unchecked((ushort)(raw + 2))))),
                    new(blockX, blockY), 0x8000), // Negative argument always closes.
            ]);

            RoomLevelData level = CreateRoom(new(8, 8),
                new ushort[64],
                new byte[64],
                blockDefinitions: new byte[0x400 * 8]);
            var system = new Bank80SystemState();
            var plms = new RoomPlmSystem();
            AssertEqual(1, plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, level.CreateBackgroundStreamer(), new SnesVram(), system, AreaId.Crateria), decoded, new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
                $"resident door $84:{raw:X4} loads through production setup");

            var enteringDoor = new CartridgeDoorHeader(
                Pointer: 0,
                DestinationRoomPointer: 0,
                Orientation: new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses),
                PlmBlock: new(blockX, blockY),
                DestinationScreen: new(0, 0),
                SamusDistance: 0,
                SetupCodePointer: DoorSetupCode.None);
            AssertTrue(plms.DoorPlms.TrySpawnDoorClosingPlm(level, enteringDoor, system),
                $"resident door $84:{raw:X4} accepts transition redirect");
            RoomPlmSlotSnapshot slot = plms.PopulationSlots.Single();
            AssertTrue(typedClosing is null
                    ? PlmProgramStartOracle.IsRunning(ResidentDoorClosingDefinitions.Resolve(header), slot)
                    : slot.Animation is ColoredDoorPlmAnimation { Program: ColoredDoorProgram.Closing }
                        or GreyDoorPlmAnimation { Program: GreyDoorProgram.Closing },
                $"resident door $84:{raw:X4} starts its closing program");
            definitionIndex++;
        }

        AssertThrows<InvalidDataException>(
            () => ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.BlueDoorFacingLeft),
            "nonresident blue-door collision header is outside resident closing domain");
    }

    /// <summary>Compares the three Speed Booster escape stages with retail control data and their runtime handoffs.</summary>
    internal static void VerifySpeedBoosterEscapeStageDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(RoomPlmProgramsVerification.VerifySpeedBoosterEscapePrograms), () => RoomPlmProgramsVerification.VerifySpeedBoosterEscapePrograms(rom));
        Suite(nameof(VerifySpeedEscapeStageTargets), () => VerifySpeedEscapeStageTargets(rom));
        Suite(nameof(VerifySpeedEscapeStageHeights), () => VerifySpeedEscapeStageHeights(rom));
        Suite(nameof(VerifySpeedEscapeStageVelocities), () => VerifySpeedEscapeStageVelocities(rom));

        // This fresh bus contains the PLM program and FX fixture, but deliberately omits
        // $84:B876-$B889. The production controller must finish all three physical stages.
        Suite(nameof(VerifySpeedBoosterEscapePlm), () => VerifySpeedBoosterEscapePlm(new TestAddressSpace()));
        Console.WriteLine(
            "Speed Booster escape definitions: three physical stages and the terminal event match.");
    }

    /// <summary>
    /// Locks the single bank-$8F parser, descending physical IDs, synchronous slot reuse,
    /// elevator animation, and all requested station setup/contact families to bounded
    /// synthetic rooms. No assertion crosses a door or depends on frontend choreography.
    /// </summary>
    internal static void VerifySequentialRoomPlmPopulationLoader()
    {
        var bus = new TestAddressSpace();
        SeedRoomPlmPopulationRom(bus);
        SeedDormantScrollPlmProgram(bus);
        const ushort population = 0x9000;
        SeedRoomPlmPopulation(bus, population, [
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
            new ushort[width * height],
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var system = new Bank80SystemState();
        var samus = new SamusState
        {
            Health = 25,
            MaxHealth = 199,
            Missiles = 1,
            MaxMissiles = 15,
        };
        var plms = new RoomPlmSystem();
        int parsed = plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, streamer, new SnesVram(), system, AreaId.Crateria), RoomPlmPopulationImporter.Read(bus, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });

        AssertEqual(4, parsed, "single parser consumes every six-byte record once");
        RoomPlmSlotSnapshot[] slots = plms.PopulationSlots.ToArray();
        AssertEqual(3, slots.Length, "extension setup deletes its preallocated actor");
        AssertEqual(39, slots[0].NativeSlotIndex, "first ROM record receives highest native slot");
        AssertEqual(0xb703, (ushort)slots[0].HeaderPointer, "first resident preserves record identity");
        AssertEqual(38, slots[1].NativeSlotIndex, "post-delete record reuses next native slot");
        AssertEqual(0xb70b, (ushort)slots[1].HeaderPointer, "elevator follows ROM order, not family order");
        AssertEqual(37, slots[2].NativeSlotIndex, "later station receives descending slot");
        AssertEqual(0xb6df, (ushort)slots[2].HeaderPointer, "station header remains observable");
        AssertEqual(1, plms.ElevatorPlatforms.Count, "elevator platform family is resident");
        AssertEqual(0x0000, level.GetCollisionBlock(new(12, 8)).LevelWord,
            "elevator setup clears collision bits through shared setup dispatcher");

        // Energy right access is parent+1 and retains solid type B while publishing the
        // parent trigger. The next PLM pass refills through live Samus state and emits the
        // cartridge message ID; it does not require a room-name callback.
        AssertEqual(0xb000, level.GetCollisionBlock(new(21, 8)).LevelWord,
            "energy setup installs type-B right access");
        AssertEqual(0x49, level.GetCollisionBlock(new(21, 8)).Behavior,
            "energy setup installs BTS $49");
        AssertTrue(plms.TryNotifyStationTouch(level.GetBlockIndex(new(21, 8)), 0x49),
            "generic BTS dispatcher locates energy-station parent");
        AssertTrue(samus.InputLocked,
            "station access setup runs command six before its first PLM handler pass");
        plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertTrue(samus.InputLocked,
            "station access command locks Samus during six-plus-$60 insertion");
        AssertTrue(plms.Sounds.SoundRequests.Contains(new PlmSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x37), 6)),
            "station access begins with cartridge extension sound $37");
        // The triggering pass installs the six-frame timer without decrementing it, so the
        // activation opcode runs on pass 1 + 6 + $60 = 103.
        for (int frame = 1; frame < 102; frame++)
            plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(25, samus.Health, "energy station has not activated before its 103rd pass");
        plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(199, samus.Health, "energy station restores health to cartridge maximum");
        AssertEqual(1, plms.Stations.StationActivationEvents.Count,
            "energy station publishes one shared message request");
        AssertEqual(GameplayMessageIds.EnergyRechargeCompleted,
            plms.Stations.StationActivationEvents[0].MessageBoxIndex,
            "energy station uses cartridge message $15");

        Suite(nameof(VerifyOtherStationFamilies), () => VerifyOtherStationFamilies(bus));
        Suite(nameof(VerifySaveStationConfirmation), () => VerifySaveStationConfirmation(bus));
        Suite(nameof(VerifyNativeOutOfBoundsPopulationSetupOrder), () => VerifyNativeOutOfBoundsPopulationSetupOrder(bus));
        Suite(nameof(VerifyMetroidsClearedStatePlm), () => VerifyMetroidsClearedStatePlm(bus));
        Suite(nameof(VerifyShaktoolRoomPlm), () => VerifyShaktoolRoomPlm());
        Suite(nameof(VerifyMotherBrainEscapeRoomGate), () => VerifyMotherBrainEscapeRoomGate(bus));
        Suite(nameof(VerifyBombTorizoGreyDoorClosingReentry), () => VerifyBombTorizoGreyDoorClosingReentry(bus));
        Suite(nameof(VerifyStandardGreyDoorClosingFallthrough), () => VerifyStandardGreyDoorClosingFallthrough(bus));
        Suite(nameof(VerifySpeedBoosterEscapePlm), () => VerifySpeedBoosterEscapePlm(bus));
        Suite(nameof(VerifyWreckedShipAtticPlm), () => VerifyWreckedShipAtticPlm(bus));
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

        SeedPlmProgramWords(bus, (ushort)PlmHeaderId.BombTorizoGreyDoor, 0xc794);
        SeedPlmProgramWords(bus, unchecked((ushort)(PlmHeaderId.BombTorizoGreyDoor + 2)),
            initialList);
        SeedPlmProgramWords(bus, unchecked((ushort)(PlmHeaderId.BombTorizoGreyDoor + 4)),
            closingList);

        // The PLM program and its draw pointers are compiled cartridge data. Do
        // not inject a shortened stand-in at their native addresses: the timing
        // and Bombs callback need to execute the same lists used in gameplay.

        SeedRoomPlmPopulation(bus, population,
        [
            0xf4, 0xba, doorX, doorY,
            unchecked((byte)roomArgument), SnesLittleEndian.HighByte(roomArgument),
            0x00, 0x00,
        ]);
        RoomLevelData level = CreateRoom(new(width, height),
            new ushort[width * height],
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var system = new Bank80SystemState();
        var samus = new SamusState
        {
            CollectedItems = SamusEquipmentFlags.Bombs.ToNativeWord(),
        };
        var plms = new RoomPlmSystem();
        AssertEqual(1, plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, streamer, new SnesVram(), system, AreaId.Crateria), RoomPlmPopulationImporter.Read(bus, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "Bomb Torizo grey door loads through the shared room population");

        var enteringDoor = new CartridgeDoorHeader(
            Pointer: 0x8bc2,
            DestinationRoomPointer: RoomHeaderPointers.BombTorizoRoom,
            Orientation: new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses),
            PlmBlock: new(doorX, doorY),
            DestinationScreen: new(0, 0),
            SamusDistance: 0x8000,
            SetupCodePointer: DoorSetupCode.None);
        AssertTrue(plms.DoorPlms.TrySpawnDoorClosingPlm(level, enteringDoor, system),
            "Bomb Torizo entry redirects the resident door to its secondary list");
        AssertEqual(GreyDoorPhase.Closing, plms.GreyDoorSnapshots.Single().Phase,
            "redirected actor retains explicit grey-door ownership while closing");

        plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }, new PlmFrameInputs { Scrolls = null, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0, CollectedItems = samus.CollectedItems });
        AssertEqual(0x0482, level.GetCollisionBlock(new(doorX, doorY)).LevelWord,
            "collected Bombs admit the secondary list's closing draw");

        for (int frame = 0; frame < 80 &&
             plms.GreyDoorSnapshots.Single().Phase == GreyDoorPhase.Closing; frame++)
            plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }, new PlmFrameInputs { Scrolls = null, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0, CollectedItems = samus.CollectedItems });
        AssertEqual(GreyDoorPhase.Locked, plms.GreyDoorSnapshots.Single().Phase,
            "closing Goto hands the resident actor back to its grey-door family");
        AssertEqual(initialList, RoomPlmProgramsVerification.GreyDoorNativeEntry(new GreyDoorVariant(ColoredDoorOrientation.Right, BombTorizo: true),
                GreyDoorProgram.Waiting),
            "rejoined actor's waiting program stands for the cartridge first list");
        AssertEqual(0xc4ae, level.GetCollisionBlock(new(doorX, doorY)).LevelWord,
            "re-entry draws the locked grey-door frame in the same handler pass");
        AssertEqual(1, plms.SlotTable.ActiveCount,
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

        SeedPlmProgramWords(bus, (ushort)PlmHeaderId.GreyDoorFacingLeft, 0xc794);
        SeedPlmProgramWords(bus, unchecked((ushort)(PlmHeaderId.GreyDoorFacingLeft + 2)),
            initialList);
        SeedPlmProgramWords(bus, unchecked((ushort)(PlmHeaderId.GreyDoorFacingLeft + 4)),
            closingList);
        SeedPlmProgramWords(bus, unchecked((ushort)(initialList + 2)), closedBlueList);
        SeedPlmProgramWords(bus, unchecked((ushort)(initialList + 6)), activationList);
        SeedPlmProgramWords(bus, unchecked((ushort)(initialList + 12)), lockedDraw);
        SeedPlmProgramWords(bus, unchecked((ushort)(activationList + 2)), openTriggerList);
        SeedPlmProgramBytes(bus, unchecked((ushort)(openTriggerList + 2)),
        [
            0x01,
            unchecked((byte)openingList),
            SnesLittleEndian.HighByte(openingList),
        ]);

        // Preserve the exact byte widths of BE59: five timer/draw pairs with an odd-sized
        // queue-sound instruction between the second and third pair. The final pair ends
        // at BE70, the start of the resident door's ordinary first list.
        SeedPlmProgramWords(bus, closingList, 2);
        SeedPlmProgramWords(bus, unchecked((ushort)(closingList + 2)), closingDraw);
        SeedPlmProgramWords(bus, unchecked((ushort)(closingList + 4)), 2);
        SeedPlmProgramWords(bus, unchecked((ushort)(closingList + 6)), closingDraw);
        SeedPlmProgramWords(bus, unchecked((ushort)(closingList + 8)),
            (ushort)RoomPlmInstruction.QueueSoundLibrary3Maximum6);
        SeedPlmProgramBytes(bus, unchecked((ushort)(closingList + 10)), [0x08]);
        SeedPlmProgramWords(bus, unchecked((ushort)(closingList + 11)), 2);
        SeedPlmProgramWords(bus, unchecked((ushort)(closingList + 13)), closingDraw);
        SeedPlmProgramWords(bus, unchecked((ushort)(closingList + 15)), 2);
        SeedPlmProgramWords(bus, unchecked((ushort)(closingList + 17)), closingDraw);
        SeedPlmProgramWords(bus, unchecked((ushort)(closingList + 19)), 1);
        SeedPlmProgramWords(bus, unchecked((ushort)(closingList + 21)), closingDraw);
        WriteOneBlockDraw(bus, closingDraw, 0xc123);
        WriteOneBlockDraw(bus, lockedDraw, 0xc123);

        SeedRoomPlmPopulation(bus, population,
        [
            unchecked((byte)PlmHeaderId.GreyDoorFacingLeft),
            SnesLittleEndian.HighByte((ushort)PlmHeaderId.GreyDoorFacingLeft),
            doorX,
            doorY,
            unchecked((byte)roomArgument),
            SnesLittleEndian.HighByte(roomArgument),
            0x00, 0x00,
        ]);
        RoomLevelData level = CreateRoom(new(width, height),
            new ushort[width * height],
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var system = new Bank80SystemState();
        var plms = new RoomPlmSystem();
        AssertEqual(1, plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, streamer, new SnesVram(), system, AreaId.Brinstar), RoomPlmPopulationImporter.Read(bus, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "Brinstar pre-map grey door loads through the shared population path");

        var enteringDoor = new CartridgeDoorHeader(
            Pointer: 0x8cb2,
            DestinationRoomPointer: 0x9b9d,
            Orientation: new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses),
            PlmBlock: new(doorX, doorY),
            DestinationScreen: new(0, 0),
            SamusDistance: 0x8000,
            SetupCodePointer: DoorSetupCode.None);
        AssertTrue(plms.DoorPlms.TrySpawnDoorClosingPlm(level, enteringDoor, system),
            "room $9B9D redirects its resident grey door to closing list $BE59");

        for (int frame = 0; frame < 10; frame++)
        {
            plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }, new PlmFrameInputs { Scrolls = null, EnemyDeaths = 0, EnemyDeathQuota = 1, ControllerNewInput = 0, CollectedItems = 0 });
        }

        AssertEqual(GreyDoorPhase.Locked, plms.GreyDoorSnapshots.Single().Phase,
            "fall-through at $BE70 returns to the resident grey-door owner");
        AssertEqual(initialList, RoomPlmProgramsVerification.GreyDoorNativeEntry(new GreyDoorVariant(ColoredDoorOrientation.Left, BombTorizo: false),
                GreyDoorProgram.Waiting),
            "the grey door's waiting program stands for the cartridge first list");
        AssertTrue(plms.PopulationSlots.Single().Animation is GreyDoorPlmAnimation { Program: GreyDoorProgram.Waiting or GreyDoorProgram.Sleep },
            "the closing falls through into the grey door's own waiting program");
        AssertEqual(1, plms.SlotTable.ActiveCount,
            "room $9B9D grey door remains resident after its entry-closing animation");
    }

    /// <summary>
    /// Reproduces the complete $BB05 lifecycle in a bounded synthetic room. Although its
    /// callback is intentionally inert, the actor must retain a native slot, install the
    /// exact $BAFA pre-instruction from ROM, and remain asleep without mutating terrain.
    /// </summary>
    private static void VerifyWreckedShipAtticPlm(TestAddressSpace bus)
    {
        const ushort population = 0x9640;
        const int width = 8;
        const int height = 8;
        const int blockX = 3;
        const int blockY = 4;

        SeedPlmProgramWords(bus, unchecked((ushort)(PlmHeaderId.WreckedShipAttic + 2)),
            RoomPlmNativeListAddresses.WreckedShipAtticEntries[0].Entry);
        SuperMetroidAddressSpace? rom = File.Exists("Super Metroid.smc")
            ? SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc")
            : null;
        if (rom is not null)
            RoomPlmProgramsVerification.VerifyWreckedShipAtticPrograms(rom);
        for (int index = 0; index < 3; index++)
            SeedPlmProgramWords(bus, checked((ushort)(RoomPlmNativeListAddresses.WreckedShipAtticEntries[0].Entry + index * 2)), 0xdead);
        SeedRoomPlmPopulation(bus, population,
        [
            0x05, 0xbb, blockX, blockY, 0x34, 0x12,
            0x00, 0x00,
        ]);

        ushort[] levelWords = new ushort[width * height];
        levelWords[blockY * width + blockX] = 0x9123;
        RoomLevelData level = CreateRoom(new(width, height),
            levelWords,
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertEqual(1, plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, level.CreateBackgroundStreamer(), new SnesVram(), new Bank80SystemState(), AreaId.WreckedShip), RoomPlmPopulationImporter.Read(bus, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "Wrecked Ship attic record is accepted by sequential loader");
        RoomPlmSlotSnapshot loaded = plms.PopulationSlots.Single();
        AssertEqual(39, loaded.NativeSlotIndex,
            "Wrecked Ship attic actor receives the native highest free slot");
        AssertTrue(loaded.Animation is WreckedShipAtticPlmAnimation, "Wrecked Ship attic actor starts its typed program");
        AssertEqual(PlmPreInstructionDefinitions.SpawnDefault, loaded.PreInstruction,
            "Wrecked Ship attic callback is not installed before its first handler pass");

        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        RoomPlmSlotSnapshot sleeping = plms.PopulationSlots.Single();
        AssertEqual(WreckedShipAtticPlmRomData.NoOpCallback, sleeping.PreInstruction,
            "Wrecked Ship attic list installs exact BAFA pre-instruction");
        AssertTrue(sleeping.Animation is WreckedShipAtticPlmAnimation { Cursor.Position: 1 },
            "Wrecked Ship attic actor sleeps permanently on its program's sleep");
        plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(1, plms.SlotTable.ActiveCount,
            "inert attic callback leaves its resident actor alive");
        AssertEqual(0x9123, level.GetCollisionBlock(new(blockX, blockY)).LevelWord,
            "inert attic callback leaves authored terrain unchanged");

        // No slot can hold an untranslated pre-instruction: a list's $84:86C1 operand is rejected where it is decoded.
        AssertThrows<InvalidDataException>(
            () => PlmPreInstructions.FromListOperand(0x9876),
            "an untranslated list-installed pre-instruction fails loudly at decode");
    }

    /// <summary>
    /// Exercises setup $B89C and all three callbacks installed by the cartridge's $B88A
    /// list. The fixture writes the explicit shared FX motion words and compiled stage records,
    /// so the test observes the same shared FX/event/earthquake owners as production.
    /// </summary>
    internal static void VerifySpeedBoosterEscapePlm(TestAddressSpace bus)
    {
        const ushort population = 0x9620;
        const int width = 16;
        const int height = 16;

        SeedPlmProgramWords(bus, 0xb8ae, RoomPlmNativeListAddresses.SpeedBoosterEscapeEntries[0].Entry);
        // Poison the old cartridge path: all three real handoffs must now use
        // the compiled list even when a synthetic address space disagrees.
        for (int index = 0; index < RoomPlmNativeListAddresses.SpeedBoosterEscapeWordCount; index++)
        {
            SeedPlmProgramWords(bus, unchecked((ushort)(RoomPlmNativeListAddresses.SpeedBoosterEscapeEntries[0].Entry + index * 2)),
                0xdead);
        }
        SeedRoomPlmPopulation(bus, population,
        [
            0xac, 0xb8, 0x01, 0x01, 0x00, 0x00,
            0x00, 0x00,
        ]);
        RoomLevelData CreateLevel() => CreateRoom(new(width, height),
            new ushort[width * height],
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);

        RoomLayer3FxState CreateFx(ushort baseY, ushort targetY, ushort velocity, byte timer)
        {
            var fx = new RoomLayer3FxState();
            fx.ApplyCartridgeMotionWrites(baseY, targetY, velocity, timer);
            return fx;
        }

        RoomPlmSystem Load(
            Bank80SystemState system,
            SamusState samus,
            RoomLayer3FxState fx,
            RoomLevelData level,
            Action<ushort> writeEarthquake)
        {
            var plms = new RoomPlmSystem();
            AssertEqual(1, plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, level.CreateBackgroundStreamer(), new SnesVram(), system, AreaId.Norfair), RoomPlmPopulationImporter.Read(bus, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Events = new FixtureEventFlags { HasEvent = system.HasEvent, SetEvent = system.SetEvent }, RoomFx = fx, Earthquake = new FixtureRoomEarthquake { WriteTimer = writeEarthquake } }),
                "Speed Booster escape record is accepted by sequential loader");
            return plms;
        }

        var completedSystem = new Bank80SystemState();
        completedSystem.SetEvent(EventNumber.OutranSpeedBoosterLavaquake);
        RoomLevelData completedLevel = CreateLevel();
        RoomPlmSystem completed = Load(
            completedSystem,
            new SamusState(),
            CreateFx(0x0300, 0x0100, 0, 5),
            completedLevel,
            _ => { });
        AssertEqual(0, completed.SlotTable.ActiveCount,
            "event-$15 setup deletes B8AC synchronously");

        var unequippedSystem = new Bank80SystemState();
        var unequippedSamus = new SamusState();
        RoomLayer3FxState unequippedFx = CreateFx(0x0300, 0x0100, 0x1234, 5);
        ushort earthquakeTimer = 7;
        RoomLevelData unequippedLevel = CreateLevel();
        RoomPlmSystem unequipped = Load(
            unequippedSystem,
            unequippedSamus,
            unequippedFx,
            unequippedLevel,
            value => earthquakeTimer = value);
        BackgroundTilemapStreamer unequippedStreamer = unequippedLevel.CreateBackgroundStreamer();
        unequipped.Step(unequippedLevel, unequippedStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        unequipped.Step(unequippedLevel, unequippedStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0, unequipped.SlotTable.ActiveCount,
            "missing Speed Booster deletes the resident controller");
        AssertEqual(ushort.MaxValue, unequippedFx.TargetYPosition,
            "missing Speed Booster disables the FX target");
        AssertEqual(0, unequippedFx.PackedYVelocity,
            "missing Speed Booster clears packed FX velocity");
        AssertEqual(0, unequippedFx.Timer.RawValue,
            "missing Speed Booster clears FX timer");
        AssertEqual(0, earthquakeTimer,
            "missing Speed Booster clears global earthquake timer");

        var disabledSystem = new Bank80SystemState();
        var disabledSamus = new SamusState
        {
            CollectedItems = (ushort)SamusEquipmentFlags.SpeedBooster,
        };
        RoomLevelData disabledLevel = CreateLevel();
        RoomPlmSystem disabled = Load(
            disabledSystem,
            disabledSamus,
            CreateFx(0x0300, ushort.MaxValue, 0, 0),
            disabledLevel,
            _ => { });
        BackgroundTilemapStreamer disabledStreamer = disabledLevel.CreateBackgroundStreamer();
        disabled.Step(disabledLevel, disabledStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        disabled.Step(disabledLevel, disabledStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0, disabled.SlotTable.ActiveCount,
            "negative FX target deletes collected-Speed-Booster controller");

        var activeSystem = new Bank80SystemState();
        var activeSamus = new SamusState
        {
            CollectedItems = (ushort)SamusEquipmentFlags.SpeedBooster,
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x0ae1, 0)),
        };
        RoomLayer3FxState activeFx = CreateFx(0x0300, 0x0100, 0, 0);
        RoomLevelData activeLevel = CreateLevel();
        RoomPlmSystem active = Load(
            activeSystem,
            activeSamus,
            activeFx,
            activeLevel,
            _ => { });
        BackgroundTilemapStreamer activeStreamer = activeLevel.CreateBackgroundStreamer();
        active.Step(activeLevel, activeStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        active.Step(activeLevel, activeStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(SpeedBoosterEscapePlmRomData.InitialLavaquakeVelocity,
            activeFx.PackedYVelocity,
            "collected Speed Booster starts the regional lavaquake velocity");
        active.Step(activeLevel, activeStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0, activeFx.Timer.RawValue,
            "Samus right of $0AE0 leaves FX motion sleeping");
        activeSamus.Position = activeSamus.Position.WithWholeX(SpeedBoosterEscapePlmRomData.StartFxMotionSamusX);
        active.Step(activeLevel, activeStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(1, activeFx.Timer.RawValue,
            "Samus reaching $0AE0 starts FX motion");

        activeSamus.Position = activeSamus.Position.WithWholeX(0x072c);
        active.Step(activeLevel, activeStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0x0300, activeFx.BaseYPosition,
            "first lava stage waits while Samus remains right of its threshold");
        activeSamus.Position = activeSamus.Position.WithWholeX(0x072b);
        active.Step(activeLevel, activeStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0x01bf, activeFx.BaseYPosition,
            "first lava stage clamps maximum FX Y");
        AssertEqual(0xff50, activeFx.PackedYVelocity,
            "first lava stage installs ROM velocity");
        activeSamus.Position = activeSamus.Position.WithWholeX(0x050a);
        active.Step(activeLevel, activeStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0x0167, activeFx.BaseYPosition,
            "second lava stage clamps maximum FX Y");
        activeSamus.Position = activeSamus.Position.WithWholeX(0x0244);
        active.Step(activeLevel, activeStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0x0100, activeFx.BaseYPosition,
            "third lava stage clamps maximum FX Y");
        active.Step(activeLevel, activeStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertTrue(activeSystem.HasEvent(EventNumber.OutranSpeedBoosterLavaquake),
            "stage-table terminator marks event $15");
        AssertEqual(1, active.SlotTable.ActiveCount,
            "native terminator leaves B8AC resident until room reload");
    }

    /// <summary>
    /// Reproduces the retail Fireflea/Colosseum defect pattern: a colored door can have a
    /// byte coordinate below the native allocation ceiling but outside the authored room.
    /// Native allocates the slot and runs setup against level_data anyway. The translation
    /// must preserve that harmless order without weakening bounds for PLMs whose setup
    /// genuinely consumes logical terrain.
    /// </summary>
    private static void VerifyNativeOutOfBoundsPopulationSetupOrder(TestAddressSpace bus)
    {
        const ushort population = 0x96c0;
        const ushort malformedPopulation = 0x96e0;
        const int width = 16;
        const int height = 32;
        const int offRoomX = 1;
        const int offRoomY = 38;
        const int inRoomX = 2;
        const int inRoomY = 2;

        var decoded = new RoomPlmPopulationDefinition(population,
        [
            new RoomPlmPlacement(RoomPlmHeaderDefinitions.Get(PlmHeaderId.GreenDoorFacingRight),
                new(offRoomX, offRoomY), 0x27),
            new RoomPlmPlacement(RoomPlmHeaderDefinitions.Get(PlmHeaderId.GreenDoorFacingLeft),
                new(inRoomX, inRoomY), 0x28),
        ]);

        RoomLevelData level = CreateRoom(new(width, height),
            new ushort[width * height],
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var plms = new RoomPlmSystem();
        AssertEqual(2, plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, streamer, new SnesVram(), new Bank80SystemState(), AreaId.Brinstar), decoded, new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "off-room colored door and following record both run setup");
        RoomPlmSlotSnapshot[] slots = plms.PopulationSlots.ToArray();
        AssertEqual(39, slots[0].NativeSlotIndex,
            "off-room record is allocated before its setup callback");
        AssertEqual(offRoomY * width + offRoomX, slots[0].BlockIndex,
            "off-room record retains native unchecked row-major block index");
        AssertEqual(38, slots[1].NativeSlotIndex,
            "following record retains descending native slot order");
        AssertEqual(0, level.GetCollisionBlockByIndex(width * height - 1).LevelWord,
            "off-room setup cannot corrupt the final authored host block");

        // Both resident actors execute their first draw. The off-room write remains in the
        // bounded native tail while the ordinary record updates logical terrain normally.
        plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0xc004, level.GetCollisionBlock(new(inRoomX, inRoomY)).LevelWord,
            "in-room record following the harmless tail actor still draws normally");

        var malformed = new RoomPlmPopulationDefinition(malformedPopulation,
        [
            new RoomPlmPlacement(RoomPlmHeaderDefinitions.Get(PlmHeaderId.ScrollTrigger),
                new(offRoomX, offRoomY), 0x9800, new byte[] { 0x80 }),
        ]);
        AssertThrows<ArgumentOutOfRangeException>(
            () => new RoomPlmSystem().PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, streamer, new SnesVram(), new Bank80SystemState(), AreaId.Brinstar), malformed, new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
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
        const ushort originalGateWord = 0xf123;
        const ushort deactivatedGateWord = 0x8123;
        const ushort openGateWord = 0x80ff;
        const ushort halfClosedGateTopWord = 0x830f;
        const ushort closedGateMiddleWord = 0x8ae8;

        SeedPlmProgramWords(bus, 0xc8ca, 0xb3c1);
        SeedPlmProgramWords(bus, 0xc8cc, RoomPlmInstructionListsTooling.MotherBrainEscapeRoomGateClosed);
        SeedPlmProgramWords(bus, 0xc8ce, RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosing);
        SeedPlmProgramWords(bus, 0xc8d0, 0xb3c1);
        SeedRoomPlmPopulation(bus, population,
        [
            0xca, 0xc8, gateX, gateY, 0x00, 0x80,
            0x00, 0x00,
        ]);

        SeedPlmProgramWords(bus, 0xbb34, 6);
        SeedPlmProgramWords(bus, 0xbb36, 0x948b);
        SeedPlmProgramWords(bus, 0xbb38, (ushort)RoomPlmInstruction.Delete);
        SeedPlmProgramWords(bus, 0xbb44, 2);
        SeedPlmProgramWords(bus, 0xbb46, 0x9473);
        SeedPlmProgramWords(bus, 0xbb48, 2);
        SeedPlmProgramWords(bus, 0xbb4a, 0x947f);
        SeedPlmProgramWords(bus, 0xbb4c, 2);
        SeedPlmProgramWords(bus, 0xbb4e, 0x948b);
        SeedPlmProgramWords(bus, 0xbb50, (ushort)RoomPlmInstruction.Delete);
        WriteVerticalPlmDraw(bus, 0x9473, [0x80ff, 0x80ff, 0x80ff, 0x80ff]);
        WriteVerticalPlmDraw(bus, 0x947f, [0x830f, 0x80ff, 0x80ff, 0x830f]);
        WriteVerticalPlmDraw(bus, 0x948b, [0x830f, 0x8ae8, 0x82e8, 0x830f]);
        var guarded = NewMotherBrainEscapeGateGuard(bus);

        RoomLevelData ordinaryLevel = CreateRoom(new(width, height),
            new ushort[width * height],
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);
        int gateBlock = ordinaryLevel.GetBlockIndex(new(gateX, gateY));
        ordinaryLevel.SetForegroundEntry(gateBlock, originalGateWord);
        BackgroundTilemapStreamer ordinaryStreamer = ordinaryLevel.CreateBackgroundStreamer();
        var ordinary = new RoomPlmSystem();
        var system = new Bank80SystemState();
        AssertEqual(1, ordinary.PopulationLoader.LoadRoomPopulation(new PlmRoom(guarded, ordinaryLevel, ordinaryStreamer, new SnesVram(), system, AreaId.Tourian), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "escape gate population record is accepted by the sequential loader");
        AssertEqual(deactivatedGateWord, ordinaryLevel.GetCollisionBlockByIndex(gateBlock).LevelWord,
            "C8CA setup clears collision class bits without deleting its resident slot");
        AssertEqual(39, ordinary.PopulationSlots.Single().NativeSlotIndex,
            "escape gate retains the highest native population slot");
        AssertEqual(RoomPlmInstructionListsTooling.MotherBrainEscapeRoomGateClosed,
            ordinary.PopulationSlots.Single().InstructionPointer,
            "ordinary entry retains C8CA's first closed-gate list");
        ordinary.Step(ordinaryLevel, ordinaryStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(closedGateMiddleWord,
            ordinaryLevel.GetCollisionBlock(new(gateX, gateY + 1)).LevelWord,
            "non-escape entry draws the cartridge's already-closed gate");
        for (int frame = 0; frame < 6; frame++)
            ordinary.Step(ordinaryLevel, ordinaryStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0, ordinary.SlotTable.ActiveCount,
            "non-escape closed-gate actor releases its slot after six authored frames");

        RoomLevelData escapeLevel = CreateRoom(new(width, height),
            new ushort[width * height],
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);
        escapeLevel.SetForegroundEntry(gateBlock, originalGateWord);
        BackgroundTilemapStreamer escapeStreamer = escapeLevel.CreateBackgroundStreamer();
        var escape = new RoomPlmSystem();
        escape.PopulationLoader.LoadRoomPopulation(new PlmRoom(guarded, escapeLevel, escapeStreamer, new SnesVram(), system, AreaId.Tourian), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
        var motherBrainExit = new CartridgeDoorHeader(
            Pointer: 0xaa8c,
            DestinationRoomPointer: 0xde4d,
            Orientation: new(DoorDirection.Left, DoorClosingBehavior.EscapeGateCloses),
            PlmBlock: new(gateX, gateY),
            DestinationScreen: new(0, 0),
            SamusDistance: 0x8000,
            SetupCodePointer: DoorSetupCode.None);
        AssertTrue(escape.DoorPlms.TrySpawnDoorClosingPlm(escapeLevel, motherBrainExit, system),
            "direction-nine Mother Brain exit redirects the resident gate");
        AssertEqual(1, escape.SlotTable.ActiveCount,
            "resident escape gate is redirected in place rather than duplicated");
        AssertEqual(39, escape.PopulationSlots.Single().NativeSlotIndex,
            "redirect preserves native room-population slot ordering");
        AssertEqual(RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosing,
            escape.PopulationSlots.Single().InstructionPointer,
            "door transition selects C8CA's second instruction list");

        escape.Step(escapeLevel, escapeStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(openGateWord, escapeLevel.GetCollisionBlock(new(gateX, gateY)).LevelWord,
            "closing frame one draws the fully open gate");
        escape.Step(escapeLevel, escapeStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(openGateWord, escapeLevel.GetCollisionBlock(new(gateX, gateY)).LevelWord,
            "two-frame gate timer retains the open image on its countdown frame");
        escape.Step(escapeLevel, escapeStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(halfClosedGateTopWord, escapeLevel.GetCollisionBlock(new(gateX, gateY)).LevelWord,
            "closing frame two draws the half-closed gate");
        escape.Step(escapeLevel, escapeStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        escape.Step(escapeLevel, escapeStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(closedGateMiddleWord, escapeLevel.GetCollisionBlock(new(gateX, gateY + 1)).LevelWord,
            "closing frame three installs the final solid gate collision");
        escape.Step(escapeLevel, escapeStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        escape.Step(escapeLevel, escapeStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0, escape.SlotTable.ActiveCount,
            "escape gate releases its slot after the final two-frame hold");

        RoomLevelData fallbackLevel = CreateRoom(new(width, height),
            new ushort[width * height],
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);
        fallbackLevel.SetForegroundEntry(gateBlock, originalGateWord);
        var fallback = new RoomPlmSystem();
        AssertTrue(fallback.DoorPlms.TrySpawnDoorClosingPlm(fallbackLevel, motherBrainExit, system),
            "special door without a resident cap spawns C8D0 fallback");
        AssertEqual(PlmHeaderId.MotherBrainEscapeRoomGateClosing,
            fallback.PopulationSlots.Single().HeaderPointer,
            "fallback actor uses the cartridge's dedicated C8D0 header");
        AssertEqual(deactivatedGateWord, fallbackLevel.GetCollisionBlockByIndex(gateBlock).LevelWord,
            "fallback C8D0 executes the shared deactivate setup");

        var nonClosingDoor = motherBrainExit with { Orientation = new(DoorDirection.Right, DoorClosingBehavior.None) };
        var nonClosing = new RoomPlmSystem();
        AssertTrue(!nonClosing.DoorPlms.TrySpawnDoorClosingPlm(fallbackLevel, nonClosingDoor, system),
            "directions zero through three retain the native no-closing-PLM branch");
        AssertEqual(0, nonClosing.SlotTable.ActiveCount,
            "non-closing door does not consume a PLM slot");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Mother Brain escape gate never rereads compiled programs or physical draws");
    }

    /// <summary>Compares the Mother Brain escape gate's compiled program, drawing and collision data with retail words.</summary>
    internal static void VerifyMotherBrainEscapeGateCompiledDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyEscapeGateProgramControls), () => VerifyEscapeGateProgramControls(rom));
        Suite(nameof(VerifyEscapeGateProgramDraws), () => VerifyEscapeGateProgramDraws(rom));
        Suite(nameof(VerifyEscapeGateDrawGeometry), () => VerifyEscapeGateDrawGeometry(rom));
        Suite(nameof(VerifyEscapeGateDrawCollision), () => VerifyEscapeGateDrawCollision(rom));
        Suite(nameof(VerifyEscapeGateDrawVisuals), () => VerifyEscapeGateDrawVisuals(rom));
    }

    /// <summary>Writes a terminated vertical PLM draw stream with one level word per cell.</summary>
    private static void WriteVerticalPlmDraw(
        TestAddressSpace bus,
        ushort pointer,
        ReadOnlySpan<ushort> levelWords)
    {
        SeedPlmProgramWords(bus, pointer, unchecked((ushort)(0x8000 | levelWords.Length)));
        for (int index = 0; index < levelWords.Length; index++)
            SeedPlmProgramWords(bus, unchecked((ushort)(pointer + 2 + index * 2)), levelWords[index]);
        SeedPlmProgramWords(bus, unchecked((ushort)(pointer + 2 + levelWords.Length * 2)), 0);
    }

    /// <summary>
    /// Exercises all semantic branches in `$DB44`'s thirteen-entry room-argument table:
    /// no-op, below-quota, four event writes, persistent sleep, and malformed input.
    /// </summary>
    internal static void VerifyMetroidsClearedStatePlm(TestAddressSpace bus)
    {
        const ushort population = 0x9500;
        SeedPlmProgramWords(bus, 0xdb46, RoomPlmNativeListAddresses.SharedEntries[1].Entry);
        SeedPlmProgramWords(bus, 0xdb42, 0xdead);
        if (File.Exists("Super Metroid.smc"))
            RoomPlmProgramsVerification.VerifySharedPrograms(
                SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"));
        SeedRoomPlmPopulation(bus, population,
        [
            0x44, 0xdb, 0x01, 0x01, 0x10, 0x00,
            0x44, 0xdb, 0x02, 0x01, 0x12, 0x00,
            0x44, 0xdb, 0x03, 0x01, 0x14, 0x00,
            0x44, 0xdb, 0x04, 0x01, 0x16, 0x00,
            0x44, 0xdb, 0x05, 0x01, 0x18, 0x00,
            0x00, 0x00,
        ]);

        RoomLevelData level = CreateRoom(new(8, 8),
            new ushort[64],
            new byte[64],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var system = new Bank80SystemState();
        var plms = new RoomPlmSystem();
        AssertEqual(5, plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, streamer, new SnesVram(), system, AreaId.Tourian), RoomPlmPopulationImporter.Read(bus, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Events = new FixtureEventFlags { HasEvent = system.HasEvent, SetEvent = system.SetEvent } }),
            "Metroids-cleared population loads all argument families");

        plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }, enemyDeaths: 2, enemyDeathQuota: 3);
        foreach (EventNumber eventNumber in new[]
                 {
                     EventNumber.FirstMetroidHallCleared,
                     EventNumber.FirstMetroidShaftCleared,
                     EventNumber.SecondMetroidHallCleared,
                     EventNumber.SecondMetroidShaftCleared,
                 })
        {
            AssertTrue(!system.HasEvent(eventNumber),
                $"Metroids-cleared {eventNumber} remains clear below quota");
        }

        plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }, enemyDeaths: 3, enemyDeathQuota: 3);
        foreach (EventNumber eventNumber in new[]
                 {
                     EventNumber.FirstMetroidHallCleared,
                     EventNumber.FirstMetroidShaftCleared,
                     EventNumber.SecondMetroidHallCleared,
                     EventNumber.SecondMetroidShaftCleared,
                 })
        {
            AssertTrue(system.HasEvent(eventNumber),
                $"Metroids-cleared {eventNumber} is marked at quota");
        }
        AssertEqual(5, plms.SlotTable.ActiveCount,
            "Metroids-cleared observers remain resident on their sleep instruction");
        AssertTrue(plms.PopulationSlots.All(slot =>
                slot.Animation is RoomPlmSharedAnimation { Program: RoomPlmSharedProgram.Sleep }),
            "Metroids-cleared observers sleep in the shared typed program");

        const ushort invalidPopulation = 0x9580;
        SeedRoomPlmPopulation(bus, invalidPopulation,
        [
            0x44, 0xdb, 0x01, 0x01, 0x01, 0x00,
            0x00, 0x00,
        ]);
        AssertThrows<InvalidDataException>(
            () => new RoomPlmSystem().PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, streamer, new SnesVram(), new Bank80SystemState(), AreaId.Tourian), RoomPlmPopulationImporter.Read(bus, invalidPopulation), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "odd Metroids-cleared room argument fails loudly");
    }

    /// <summary>
    /// The hardcoded room-setup spawn and actual PLM handler must install the
    /// compiled $B8D6 callback before the power-bomb and path-clear branches.
    /// </summary>
    private static void VerifyShaktoolRoomPlm()
    {
        var bus = new TestAddressSpace();
        SuperMetroidAddressSpace? rom = File.Exists("Super Metroid.smc")
            ? SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc")
            : null;
        for (int index = 0; index < 3; index++)
        {
            ushort address = checked((ushort)(ShaktoolRoomPlmRomData.InstructionList + index * 2));
            AssertTrue(ShaktoolRoomPlmRomData.TryReadInstructionWord(address, out ushort compiled),
                $"Shaktool room list word ${address:X4} is compiled");
            if (rom is not null)
            {
                int source = 0x840000 | address;
                AssertEqual(ReadRetailPlmProgramWord(rom, source),
                    compiled,
                    $"Shaktool room list word ${address:X4} matches pinned cartridge");
            }
            SeedPlmProgramWords(bus, address, 0xdead);
        }
        AssertTrue(!ShaktoolRoomPlmRomData.TryReadInstructionWord(
                checked((ushort)(ShaktoolRoomPlmRomData.InstructionList + 6)), out _),
            "Shaktool room list owner excludes adjacent setup code");

        const ushort emptyPopulation = 0x9780;
        SeedRoomPlmPopulation(bus, emptyPopulation, [0x00, 0x00]);
        RoomLevelData level = CreateRoom(new(32, 32), new ushort[32 * 32],
            new byte[32 * 32], blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        RoomScrollGrid scrolls = RoomScrollGrid.LoadCompiled(new RoomScrollStorage(),
            new byte[RoomScrollGrid.StorageByteCount], new Extent16<RoomScreens>(2, 2));
        var samus = new SamusState();
        var system = new Bank80SystemState();
        var plms = new RoomPlmSystem();
        AssertEqual(0, plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, streamer, new SnesVram(), system, AreaId.Maridia), RoomPlmPopulationImporter.Read(bus, emptyPopulation), new RoomPlmServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Events = new FixtureEventFlags { HasEvent = system.HasEvent, SetEvent = system.SetEvent } }),
            "empty fixture reserves no PLM before Shaktool setup spawn");
        AssertTrue(plms.Shaktool.TrySpawnShaktoolRoomController(scrolls),
            "Shaktool room setup spawns its resident controller");
        AssertEqual(RoomScrollState.Blue, scrolls.ReadState(0),
            "Shaktool setup opens the first scroll cell");
        AssertEqual(RoomScrollState.RedBoundary, scrolls.ReadState(3),
            "Shaktool setup keeps later scroll cells red");

        plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }, new PlmFrameInputs { Scrolls = scrolls, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0 });
        RoomPlmSlotSnapshot sleeping = plms.PopulationSlots.Single();
        AssertEqual(ShaktoolRoomPlmRomData.PreInstruction, sleeping.PreInstruction,
            "compiled list installs Shaktool room pre-instruction");
        AssertEqual(checked((ushort)(ShaktoolRoomPlmRomData.InstructionList + 4)),
            sleeping.InstructionPointer,
            "compiled list sleeps on its last instruction");

        plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }, new PlmFrameInputs { Scrolls = scrolls, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0, PowerBombExplosionStatus = 1 });
        for (int cell = 0; cell < ShaktoolRoomPlmRomData.ScrollCellCount; cell++)
            AssertEqual(RoomScrollState.Blue, scrolls.ReadState(cell),
                $"Shaktool power bomb opens scroll cell {cell}");
        AssertEqual(1, plms.SlotTable.ActiveCount,
            "Shaktool controller remains resident until Samus passes the threshold");

        samus.Position = samus.Position.WithWholeX(checked((ushort)(ShaktoolRoomPlmRomData.ClearedPathXBoundary + 1)));
        plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }, new PlmFrameInputs { Scrolls = scrolls, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0 });
        AssertTrue(system.HasEvent(EventNumber.ShaktoolClearedPath),
            "Shaktool path event follows the native X threshold");
        AssertEqual(0, plms.SlotTable.ActiveCount,
            "Shaktool controller deletes itself when the path event is set");
    }

    /// <summary>Exercises map, missile and save-station admission and message timing in one populated room.</summary>
    internal static void VerifyOtherStationFamilies(TestAddressSpace bus,
        RoomPlmStationVisualCatalog? visuals = null)
    {
        const ushort population = 0x9200;
        SeedRoomPlmPopulation(bus, population, [
            0xd3, 0xb6, 0x06, 0x06, 0x00, 0x00,
            0xeb, 0xb6, 0x0c, 0x06, 0x00, 0x00,
            0x6f, 0xb7, 0x12, 0x06, 0x03, 0x00,
            0x00, 0x00,
        ]);
        const int width = 32;
        RoomLevelData level = CreateRoom(new(width, 16),
            new ushort[width * 16],
            new byte[width * 16],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var system = new Bank80SystemState();
        var samus = new SamusState { Health = 99, MaxHealth = 99, Missiles = 0, MaxMissiles = 10 };
        var plms = new RoomPlmSystem { Drawing = { StationVisuals = visuals } };
        plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, streamer, new SnesVram(), system, AreaId.Norfair), RoomPlmPopulationImporter.Read(bus, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });

        AssertEqual(3, plms.StationSnapshots.Count, "map, missile, and save parents share one pool");
        AssertEqual(0x47, level.GetCollisionBlock(new(7, 6)).Behavior,
            "map right access setup uses BTS $47");
        AssertEqual(0x48, level.GetCollisionBlock(new(4, 6)).Behavior,
            "map left access setup uses native two-block offset");
        AssertEqual(0x4b, level.GetCollisionBlock(new(13, 6)).Behavior,
            "missile right access setup uses BTS $4B");
        AssertEqual(0x4c, level.GetCollisionBlock(new(11, 6)).Behavior,
            "missile left access setup uses BTS $4C");
        AssertEqual(0x4d, level.GetCollisionBlock(new(18, 6)).Behavior,
            "save station setup installs trigger BTS $4D");

        AssertTrue(plms.TryNotifyStationTouch(level.GetBlockIndex(new(7, 6)), 0x47),
            "map access resolves parent");
        AssertTrue(samus.InputLocked,
            "map access setup locks Samus before the resident PLM advances");
        AssertTrue(plms.TryNotifyStationTouch(level.GetBlockIndex(new(13, 6)), 0x4b),
            "missile access resolves parent");
        // Activation runs on the triggering pass plus 6 + $60 later passes.
        for (int frame = 0; frame < 102; frame++)
            plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertTrue(!system.HasAreaMap(2), "map station has not activated before its 103rd pass");
        plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0xb859, level.GetCollisionBlock(new(18, 6)).LevelWord,
            "compiled save-pod idle draw installs its physical floor word");
        AssertEqual(0x005b, level.GetCollisionBlock(new(18, 2)).LevelWord,
            "compiled save-pod draw extends its shaft above the trigger");
        AssertEqual(0x8059, level.GetCollisionBlock(new(18, 1)).LevelWord,
            "compiled save-pod draw installs its upper cap");
        AssertTrue(system.HasAreaMap(2), "map station marks current area acquired");
        AssertEqual(10, samus.Missiles, "missile station restores missiles");
        AssertEqual(2, plms.Stations.StationActivationEvents.Count,
            "two station parents publish two explicit activations");
        // $84:8CE7 runs Samus command one as soon as the missile message returns; the map
        // activation never unlocks, but both fixtures share this Samus owner.
        AssertTrue(!samus.InputLocked,
            "missile activation releases Samus after its completion message");

        // The runtime freezes this PLM while the message is open. Once bank $85 returns,
        // the original instruction lists execute three six-frame phases: post-message
        // hold, retract, and final hold, without looping the access sound.
        for (int frame = 0; frame < 18; frame++)
            plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0x8128, level.GetCollisionBlock(new(7, 6)).LevelWord,
            "map access retracts to its compiled right-side parent word");
        AssertEqual(0x8528, level.GetCollisionBlock(new(4, 6)).LevelWord,
            "map access retracts its linked left-side block");
        AssertEqual(0xb4c3, level.GetCollisionBlock(new(13, 6)).LevelWord,
            "missile access retracts to its compiled resource word");
        AssertTrue(!samus.InputLocked,
            "Samus stays released through the post-message retraction");

        AssertTrue(plms.TryNotifyStationTouch(level.GetBlockIndex(new(18, 6)), 0x4d),
            "save trigger resolves its same-block parent");
        plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(StationKind.Save, plms.Stations.StationActivationEvents.Single().Kind,
            "save trigger publishes typed confirmation request");
        AssertEqual(3, plms.Stations.StationActivationEvents.Single().StationIndex,
            "save request preserves native room argument/station index");

        StationActivationEvent saveRequest = plms.Stations.StationActivationEvents.Single();
        samus.Position = samus.Position.WithWholeX(0x0127);
        ushort[] levelBeforeSave = Enumerable.Range(0, level.Size.X * level.Size.Y)
            .Select(block => level.GetCollisionBlock(new(checked((ushort)(block % level.Size.X)), checked((ushort)(block / level.Size.X)))).LevelWord)
            .ToArray();
        SaveStationConfirmationResult confirmation =
            plms.Stations.ResolveSaveStationConfirmation(saveRequest, accepted: true, level: level, streamer: streamer, view: new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertTrue(confirmation.Saving, "accepted save resumes the sleeping cartridge PLM");
        // #1269: $84:AFF4-$AFFA run in the PLM pass the message returned into.
        AssertTrue(plms.Sounds.SoundRequests.Contains(new PlmSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x2e), 6)),
            "the confirming pass queues cartridge library-one sound $2E");
        AssertTrue(Enumerable.Range(0, levelBeforeSave.Length).Any(block => levelBeforeSave[block] !=
                level.GetCollisionBlock(new(checked((ushort)(block % level.Size.X)), checked((ushort)(block / level.Size.X)))).LevelWord),
            "the confirming pass draws the first electricity frame");
        AssertEqual(0x0120, samus.Position.X.Whole,
            "save animation centers Samus on the station's 16-pixel boundary");
        AssertTrue(SamusPoseMetadata.IsForwardFacingPose(samus.Pose),
            "save animation applies shared MakeSamusFaceForward setup");
        AssertTrue(samus.InputLocked, "save animation owns Samus input");
        AssertEqual(level.GetBlockIndex(new(18, 6)), saveRequest.BlockIndex,
            "save activation retains the executing PLM block for bank-$86 electricity");
        Suite(nameof(VerifySaveStationElectricity), () => VerifySaveStationElectricity(bus, level, saveRequest.BlockIndex));

        StationActivationEvent completion = default;
        bool completionPublished = false;
        int completionFrame = -1;
        int maximumSaveAnimationFrames =
            SaveStationAnimationDefinitions.SaveAnimationLoops * 8 + 8;
        for (int frame = 0; frame < maximumSaveAnimationFrames && !completionPublished; frame++)
        {
            plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
            if (plms.Stations.StationActivationEvents.Count != 0)
            {
                completion = plms.Stations.StationActivationEvents.Single();
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
        plms.Stations.CompleteSaveStation(completion);
        AssertTrue(!samus.InputLocked, "game-saved message return unlocks Samus");
        AssertTrue(plms.StationSnapshots.Single(station => station.Kind == StationKind.Save)
            .SaveStationLockedOut,
            "save completion installs once-per-room-entry lockout");

        AssertTrue(plms.TryNotifyStationTouch(level.GetBlockIndex(new(18, 6)), 0x4d),
            "locked save trigger remains a solid station block");
        plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0, plms.Stations.StationActivationEvents.Count,
            "locked save station cannot reopen until room re-entry");
    }

    /// <summary>Checks the save station's enemy-projectile electricity art and placement at its room block.</summary>
    private static void VerifySaveStationElectricity(
        TestAddressSpace bus,
        RoomLevelData level,
        int saveBlockIndex)
    {
        // The native control stream is compiled. Constructed artwork is supplied by
        // operand identity, rather than shortening a bank-$86 program on a fake bus.
        SeedEmptyRoomEnemyPopulation(bus, 0x9600);
        SeedSaveStationElectricityProjectile(bus);

        var enemies = new RoomEnemySystem();
        enemies.Load(new RoomEnemyLoadRequest(bus, 0x9600, 0, new SnesVram(), new SnesCgram(), new StandaloneRoomEnemyServices(() => 0x4040)) { Level = level, Samus = new SamusState() });
        enemies.Projectiles.SpawnSaveStationElectricity(saveBlockIndex, level.Size.X);

        var visible = new SpriteVisualPart
        {
            OffsetX = 0, OffsetY = 0, TileColumn = 1, TileRow = 0,
            Size = 8, Priority = 1, Palette = 0, FlipX = false, FlipY = false,
        };
        HashSet<ushort> electricityOperands = Enumerable.Range(0, SaveStationElectricityFrameDefinitions.PoseCount)
            .Select(index => SaveStationElectricityFrameDefinitions.PresentationKey(new(index))).ToHashSet();
        var artworkDocument = new EnemyProjectileSpritemapDocument
        {
            Version = (int)EnemyProjectileSpritemapVersion.Current,
            Frames = EnemyProjectileSpritemapDefinitions.Frames.ToDictionary(frame => frame.Item2, _ => Array.Empty<SpriteVisualPart>()),
            ProgramFrames = EnemyProjectilePresentationFrameDefinitions.All.ToDictionary(
                frame => frame.Name, frame => electricityOperands.Contains(frame.OperandAddress) ? new[] { visible } : Array.Empty<SpriteVisualPart>()),
        };
        enemies.TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(), ProjectileSpritemaps = EnemyProjectileSpritemapCatalog.Load(new MemoryStream(
                EnemyProjectileSpritemapCatalog.Write(artworkDocument))) });

        RoomEnemyProjectileSlot electricity = enemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.SaveStationElectricity);
        AssertEqual(0x0130, electricity.Position.X.Whole,
            "save electricity starts one block right of the station PLM");
        AssertEqual(0x0040, electricity.Position.Y.Whole,
            "save electricity starts two blocks above the station PLM");

        enemies.StepEnemyProjectiles(level, samus: null);
        AssertEqual(SaveStationElectricityFrameDefinitions.PresentationKey(new(0)),
            electricity.PresentationOperandAddress, "save electricity selects its first installed frame through its typed program");
        AssertEqual(EnemyProjectileSpritemapDefinitions.BlankSpritemap, electricity.SpritemapPointer,
            "installed program artwork does not retain a cartridge spritemap pointer");
        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawEnemyProjectiles(oam, new(0x0100, 0));
        oam.FinalizeFrame();
        AssertTrue(Enumerable.Range(0, oam.LastFinalizedSpriteCount)
            .Select(oam.GetEntry)
            .Any(entry => entry.X == 0x0030 && entry.Y == 0x40),
            "save electricity reaches the ordinary room enemy-projectile draw path");
    }

    /// <summary>Checks that an unsupported resident header reports its room pointer, record, block and argument at load time.</summary>
    private static void VerifyUnsupportedPopulationContext(TestAddressSpace bus)
    {
        const ushort population = 0x9400;
        const ushort unsupportedHeader = 0xb123;
        SeedPlmProgramWords(bus, unsupportedHeader, 0x9876);
        SeedPlmProgramWords(bus, unchecked((ushort)(unsupportedHeader + 2)), 0x9abc);
        SeedRoomPlmPopulation(bus, population, [
            0x23, 0xb1, 0x03, 0x04, 0x55, 0xaa, 0x00, 0x00,
        ]);
        RoomLevelData level = CreateRoom(new(8, 8),
            new ushort[64],
            new byte[64],
            blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        NotSupportedException? error = null;
        try
        {
            plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, level.CreateBackgroundStreamer(), new SnesVram(), new Bank80SystemState(), 0), RoomPlmPopulationImporter.Read(bus, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
        }
        catch (NotSupportedException caught)
        {
            error = caught;
        }
        AssertTrue(error is not null,
            "unsupported room-population header fails at load time");
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
        GameplayMessageTitleCell[] Cells(int count, ushort first) => Enumerable.Range(0, count)
            .Select(index => new GameplayMessageTitleCell { Raw = unchecked((ushort)(first + index)) }).ToArray();
        var notices = new GameplayMessageNoticeDocument
        {
            Version = GameplayMessageNoticeDefinitions.Version,
            Border = Cells(GameplayMessageRomData.Layout.TilemapWidth, 0x3801),
            Notices = GameplayMessageNoticeDefinitions.MessageIds.ToDictionary(id => id.ToString(), id =>
                new GameplayMessageNotice
                {
                    RowCount = GameplayMessageNoticeDefinitions.ContentRows(id),
                    Template = Cells(GameplayMessageNoticeDefinitions.ContentRows(id) * GameplayMessageRomData.Layout.TilemapWidth, 0x2800),
                    Text = [new GameplayMessageTextRegion { Row = 0, Column = 0, Width = 4,
                        Alignment = GameplayMessageNoticeDefinitions.LeftAlignment, Text = "TEST", Palette = 0 }],
                    YesSelection = GameplayMessageNoticeDefinitions.IsSaveConfirmation(id) ? Cells(32, 0x5100) : null,
                    NoSelection = GameplayMessageNoticeDefinitions.IsSaveConfirmation(id) ? Cells(32, 0x5200) : null,
                }),
        };
        using var noticeJson = new MemoryStream();
        GameplayMessageNoticePresentation.Write(noticeJson, notices);
        noticeJson.Position = 0;
        GameplayMessageNoticePresentation presentation = GameplayMessageNoticePresentation.Load(noticeJson);
        SeedSaveConfirmationMessageCartridge(bus);

        var message = new GameplayMessageBoxState();
        message.BindPresentation(null, notices: presentation);
        message.Begin(GameplayMessageIds.SaveConfirmation, 0);
        for (int guard = 0;
             message.Phase != GameplayMessageBoxPhase.AwaitingInput && guard < 32;
             guard++)
        {
            message.Step(0);
        }
        AssertEqual(GameplayMessageBoxPhase.AwaitingInput, message.Phase,
            "save message enters shared yes/no selection phase");
        AssertEqual(0x5100, message.Tilemap[128],
            "save message begins with native selected-YES row");
        // $85:84BA waits two lag frames before its first ReadControllerInput.
        message.Step(0);
        message.Step(SnesButton.Left);
        AssertTrue(message.ConfirmationSelectionYes, "save selector does not read during its lag waits");
        message.Step(SnesButton.Left);
        AssertTrue(!message.ConfirmationSelectionYes,
            "save cursor changes the shared confirmation selection");
        AssertEqual(0x5200, message.Tilemap[128],
            "save cursor redraws the native selected-NO row");
        // Later reads come every second frame: the read's frame holds the next first wait.
        // A toggle's redraw wait precedes that cycle once (#1269), so the first read after
        // it comes three frames later.
        message.Step(SnesButton.Left);
        message.Step(SnesButton.Left);
        message.Step(SnesButton.Left);
        AssertTrue(!message.ConfirmationSelectionYes,
            "holding a direction does not toggle the newly-pressed save cursor repeatedly");
        message.Step(SnesButton.A);
        AssertEqual(GameplayMessageBoxPhase.AwaitingInput, message.Phase, "A is read on the next read frame");
        message.Step(SnesButton.A);
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
        message.Step(SnesButton.A);
        for (int guard = 0; message.IsActive && guard < 32; guard++)
            message.Step(0);
        AssertEqual(true, message.ConsumeConfirmationResult(),
            "save confirmation defaults to yes and publishes acceptance");
    }

    /// <summary>Seeds the sparse bank-$84 header pointers and elevator frame loop needed by population fixtures.</summary>
    internal static void SeedRoomPlmPopulationRom(TestAddressSpace bus)
    {
        SeedPoseOneSamusData(bus);
        // Header instruction-list fields consumed by the generic allocator.
        SeedPlmProgramWords(bus, 0xb705, 0xaf86);
        SeedPlmProgramWords(bus, 0xb63d, 0xafa4);
        SeedPlmProgramWords(bus, 0xb70d, 0xafb6);
        SeedPlmProgramWords(bus, 0xb6e1, 0xadc2);
        SeedPlmProgramWords(bus, 0xb6d5, 0xad62);
        SeedPlmProgramWords(bus, 0xb6ed, 0xae4c);
        SeedPlmProgramWords(bus, 0xb771, 0xafe8);
        // Elevator's four synthetic frames. Station timing, pointer selection, and
        // draw payloads are compiled, so this sparse bus carries none of those bytes.
        SeedTimedDrawLoop(bus, 0xafb6, 4, 4, 0xa100);
        SeedPlmProgramWords(bus, 0xafc6, 0x8724);
        SeedPlmProgramWords(bus, 0xafc8, 0xafb6);
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
            SeedPlmProgramWords(bus, unchecked((ushort)(list + frame * 4)), timer);
            SeedPlmProgramWords(bus, unchecked((ushort)(list + frame * 4 + 2)), draw);
            WriteOneBlockDraw(bus, draw, unchecked((ushort)(0x8000 | frame)));
        }
    }

    /// <summary>Rejects runtime rereads of compiled Mother Brain gate control and draw bytes.</summary>
    internal static CartridgeReadGuard NewMotherBrainEscapeGateGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(
            address => address is >= 0x84bb34 and <= 0x84bb51 or >= 0x849473 and <= 0x849496,
            address => $"Mother Brain escape gate reread compiled source ${address:X6}.");

    /// <summary>The dormant scroll fixture owns an empty terminated bank-$8F scroll program at $9100.</summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for an empty bank-$8F scroll PLM program")]
    private static void SeedDormantScrollPlmProgram(TestAddressSpace bus)
    {
        bus.WriteBytes(0x8f9100, [0x80]); // Dormant scroll fixture owns an empty terminated program.
    }

    /// <summary>Seeds the save-station electricity enemy-projectile definition, animation list and frame map.</summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for the save-station electricity enemy projectile")]
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
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for the save-confirmation message definition and tilemaps")]
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
    internal static int RoomPlmPopulationCommand()
    {
     VerifySequentialRoomPlmPopulationLoader(); return 0; 
    }

    /// <summary>Runs retail door-closing definition checks through the command dispatcher.</summary>
    [VerificationCommand("--door-closing-definitions")]
    internal static int DoorClosingDefinitionsCommand()
    {
        Suite(nameof(VerifyDoorClosingPlmDefinitions), () => VerifyDoorClosingPlmDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
