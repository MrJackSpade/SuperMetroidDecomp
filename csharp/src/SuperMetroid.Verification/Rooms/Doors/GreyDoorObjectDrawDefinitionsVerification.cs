using SuperMetroid.AssetExtraction;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.LookupGreyCapVerification;

using static SuperMetroid.Verification.Rooms.Doors.LookupGreyDoorStockVerification;
using SuperMetroid.Verification.Rooms.RoomObjects;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.Doors;
using SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Retail and guarded runtime checks for ordinary and Bomb Torizo grey-door PLM programs and editable draw blocks.</summary>
internal static class GreyDoorObjectDrawDefinitionsVerification
{
    /// <summary>Checks native program words, five door lifecycles, physical draw ownership and installed visuals.</summary>
    internal static void VerifyGreyDoorObjectDrawDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyGreyDoorProgramDefinitions), () => VerifyGreyDoorProgramDefinitions(rom));
        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            LittleEndianBytes.Word(GreyDoorObjectDrawDefinitionsRomByte(bus, address), GreyDoorObjectDrawDefinitionsRomByte(bus, address + 1));

        RoomObjectShotBlockDrawDefinitions.DrawList[] lists =
            RoomObjectGreyDoorVisualFiles.All.OrderBy(list => RoomObjectDrawAddressCodec.Of(list.Draw)).ToArray();
        InstalledRoomObjectGreyDoorVisuals stock = InstalledRoomObjectGreyDoorVisuals.Stock();
        AssertEqual("grey-right-frame-0", RoomObjectGreyDoorVisualFiles.VisualId(NativeObjectDraws.At<GreyDoorCaps.Draw>(0xa6d7)),
            "the grey right cap's first frame is the draw the cartridge stores at $A6D7");
        AssertEqual(RoomLevelWordCodec.Decode(ReadWord(rom, 0x840000 | (0xa6d7 + 2))).Appearance,
            stock.Image(NativeObjectDraws.At<GreyDoorCaps.Draw>(0xa6d7)).At(Offset16<RoomBlocks>.Zero),
            "stock grey-door visual retains native tile choice");
        Suite(nameof(VerifyGreyCapGeometry), () => VerifyGreyCapGeometry(rom));
        Suite(nameof(VerifyGreyCapCollision), () => VerifyGreyCapCollision(rom));
        Suite(nameof(VerifyGreyCapVisuals), () => VerifyGreyCapVisuals(rom));

        var bank84 = new byte[0x8000];
        for (int offset = 0; offset < bank84.Length; offset++)
            bank84[offset] = GreyDoorObjectDrawDefinitionsRomByte(rom, 0x848000 + offset);
        RoomObjectKind[] headers =
        [
            RoomObjectKind.BombTorizoGreyDoor,
            RoomObjectKind.GreyDoorFacingLeft,
            RoomObjectKind.GreyDoorFacingRight,
            RoomObjectKind.GreyDoorFacingUp,
            RoomObjectKind.GreyDoorFacingDown,
        ];
        foreach (RoomObjectKind header in headers)
        {
            TestAddressSpace bus = CreateBank84Cartridge(bank84);
            const ushort population = 0x9000;
            SeedRoomObjectPopulation(bus, population,
            [
                unchecked((byte)RoomObjectNativeWords.Word(header)), LittleEndianBytes.HighByte(RoomObjectNativeWords.Word(header)),
                4, 4,
                header == RoomObjectKind.BombTorizoGreyDoor ? (byte)0x1b : (byte)0,
                header == RoomObjectKind.BombTorizoGreyDoor ? (byte)0x08 : (byte)0,
                0, 0,
            ]);
            const int width = 16;
            const int origin = 4 + 4 * width;
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new RoomTerrainBlock[width * width], new RoomBlockBehavior[width * width], new RoomBlockAppearance[width * width]), RoomLevelData.NothingBeyondGrid());
            var roomObjects = RoomObjectFixtures.Unbound();
            var system = new SharedGameState(new SharedRandom());
            var guarded = NewGreyDoorDrawGuard(bus, lists);
            AssertEqual(1, RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, BgArtworkFixtures.For(new SnesVram()), system, AreaId.Crateria), RoomObjectPopulationImporter.Read(guarded, population), new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
                $"resident grey-door header {header} loads");
            ushort initial = ReadWord(rom, 0x840000 | (RoomObjectNativeWords.Word(header) + 2));
            ushort firstDraw = ReadWord(rom, 0x840000 | (initial + 12));
            AssertTrue(RoomObjectGreyDoorVisualFiles.TryGet(NativeObjectDraws.At<GreyDoorCaps.Draw>(firstDraw), out var selected),
                $"resident grey-door header {header} selects compiled art");
            roomObjects.Step(level);
            bool vertical = selected.Runs.Span[0].Direction == RoomObjectRunDirection.Vertical;
            int stride = vertical ? width : 1;
            for (int block = 0; block < 4; block++)
            {
                RoomTerrainBlock written = level.GetCollisionBlockByIndex(origin + block * stride).Terrain;
                AssertEqual(selected.Runs.Span[0].Collisions.Span[block], written.Collision,
                    $"resident grey-door {header} draws physical block {block}");
                AssertEqual(stock.Image(selected.Draw).At(selected.Runs.Span[0].CellAt(block)), written.Appearance,
                    $"resident grey-door {header} shows its installed picture at block {block}");
            }

            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"resident grey-door {header} avoids draw payload ROM reads");
            if (header == RoomObjectKind.BombTorizoGreyDoor)
            {
                VerifyBombTorizoGreyDoorLifecycle(guarded, new TorizoGreyDoorRoom { Level = level, RoomObjects = roomObjects, System = system }, population, origin);
            }
            else
            {
                var enteringDoor = new DoorDefinition(
                    Name: "fixture door",
                    DestinationRoom: FixtureRoomHeaders.UnloadedDestination,
                    Orientation: new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses),
                    RoomObjectBlock: new(4, 4),
                    DestinationScreen: new(0, 0),
                    SamusSpeed: DoorSamusSpeed.None,
                    SetupCode: DoorSetupCode.None);
                AssertTrue(roomObjects.DoorObjects.TrySpawnDoorClosingObject(level, enteringDoor,
                        system),
                    $"resident grey door {header} selects its closing list");
                for (int frame = 0; frame < 20 &&
                     roomObjects.GreyDoorSnapshots.Single().Phase == GreyDoorPhase.Closing; frame++)
                    roomObjects.Step(level);
                AssertEqual(GreyDoorPhase.Locked, roomObjects.GreyDoorSnapshots.Single().Phase,
                    $"resident grey door {header} returns to locked owner");

                // The default room argument selects the boss-defeated condition. A
                // closed door must flash only after the cartridge event bit changes.
                system.SetBossBits(AreaId.Crateria, BossBits.AreaBoss);
                roomObjects.Step(level);
                AssertEqual(GreyDoorPhase.Flashing, roomObjects.GreyDoorSnapshots.Single().Phase,
                    $"resident grey door {header} unlocks after boss event");
                AssertTrue(roomObjects.TryNotifyColoredDoorHit(level.BlockAt(origin),
                        SamusProjectileType.None),
                    $"resident grey door {header} accepts a beam hit");
                roomObjects.Step(level);
                AssertEqual(GreyDoorPhase.Opening, roomObjects.GreyDoorSnapshots.Single().Phase,
                    $"resident grey door {header} enters opening list");
                for (int frame = 0; frame < 128 && roomObjects.SlotTable.ActiveCount != 0; frame++)
                    roomObjects.Step(level);
                AssertEqual(0, roomObjects.SlotTable.ActiveCount,
                    $"resident grey door {header} completes and deletes");
                AssertTrue(system.HasOpenedDoorBit(0),
                    $"resident grey door {header} persists opened bit");
                AssertEqual(0, guarded.ForbiddenReadAttempts,
                    $"resident grey door {header} never rereads program or draw data");

                var reopenedLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new RoomTerrainBlock[width * width], new RoomBlockBehavior[width * width], new RoomBlockAppearance[width * width]), RoomLevelData.NothingBeyondGrid());
                var reopened = RoomObjectFixtures.Unbound();
                AssertEqual(1, RoomObjectFixtures.LoadPopulation(ref reopened, new RoomObjectRoom(reopenedLevel, BgArtworkFixtures.For(new SnesVram()), system, AreaId.Crateria), RoomObjectPopulationImporter.Read(guarded, population), new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
                    $"opened grey door {header} reloads");
                // The closed blue list shows the blue cap for one frame and deletes on the next pass.
                reopened.Step(reopenedLevel);
                AssertTrue(reopened.PopulationSlots.Single().Behaviour is BlueDoorCapObject { Phase: BlueDoorCapPhase.Closed },
                    $"opened grey door {header} converts to blue cap");
                reopened.Step(reopenedLevel);
                AssertEqual(0, reopened.SlotTable.ActiveCount,
                    $"opened grey door {header} deletes after its blue frame");
                AssertEqual(0, guarded.ForbiddenReadAttempts,
                    $"opened grey door {header} avoids program/draw ROM reads");
            }
        }
        Suite(nameof(VerifyGreyDoorVisualInstallation), () => VerifyGreyDoorVisualInstallation(rom));
        Console.WriteLine(
            "  Grey doors: 420 ordinary and 117 Bomb Torizo program bytes, five close/unlock/open/reload paths, 20 physical draws, and editable visual blocks pass with source reads blocked.");
    }

    /// <summary>Reads retail draw and program bytes only for the comparison oracle.</summary>
    private static byte GreyDoorObjectDrawDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Blocks compiled ordinary, Bomb Torizo and draw-list byte ranges during resident-door playback.</summary>
    private static CartridgeReadGuard NewGreyDoorDrawGuard(
        ISnesAddressSpace source,
        RoomObjectShotBlockDrawDefinitions.DrawList[] lists) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x84be59 and <= 0x84bffc,
                address => $"Resident grey door reread compiled instruction ${address:X6}.")
            .Forbid(address => address is >= 0x84ba4c and <= 0x84ba6e or >= 0x84ba7f and <= 0x84bad0,
                address => $"Bomb Torizo grey door reread compiled instruction ${address:X6}.")
            .Forbid(address => lists.Any(list =>
                    address >= (0x840000 | RoomObjectDrawAddressCodec.Of(list.Draw)) &&
                    address < (0x840000 | RoomObjectDrawAddressCodec.Of(list.Draw)) + 12),
                address => $"Resident grey door reread draw payload ${address:X6}.");

    /// <summary>Advances the Bomb-gated door through collection, defeat, hit, opening and persisted blue-cap reload.</summary>
    private static void VerifyBombTorizoGreyDoorLifecycle(CartridgeReadGuard guarded, TorizoGreyDoorRoom room, ushort population, int origin)
    {
        var enteringDoor = new DoorDefinition(
            Name: "fixture door",
            DestinationRoom: FixtureRoomHeaders.UnloadedDestination,
            Orientation: new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses),
            RoomObjectBlock: new(4, 4),
            DestinationScreen: new(0, 0),
            SamusSpeed: DoorSamusSpeed.None,
            SetupCode: DoorSetupCode.None);
        AssertTrue(room.RoomObjects.DoorObjects.TrySpawnDoorClosingObject(room.Level, enteringDoor, room.System),
            "Bomb Torizo door selects its Bomb-gated closing list");
        for (int frame = 0; frame < 10; frame++)
            room.RoomObjects.Step(room.Level);
        AssertEqual(GreyDoorPhase.Closing, room.RoomObjects.GreyDoorSnapshots.Single().Phase,
            "Bomb Torizo closing list loops while Samus has no Bombs");
        for (int frame = 0; frame < 80 &&
             room.RoomObjects.GreyDoorSnapshots.Single().Phase == GreyDoorPhase.Closing; frame++)
            room.RoomObjects.Step(room.Level, new RoomObjectFrameInputs { Scrolls = null, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0, CollectedItems = SamusEquipmentFlags.Bombs });
        AssertEqual(GreyDoorPhase.Locked, room.RoomObjects.GreyDoorSnapshots.Single().Phase,
            "collected Bombs let the special closing list rejoin its resident owner");
        room.System.SetBossBits(AreaId.Crateria, BossBits.AreaTorizo);
        room.RoomObjects.Step(room.Level);
        AssertEqual(GreyDoorPhase.Flashing, room.RoomObjects.GreyDoorSnapshots.Single().Phase,
            "Bomb Torizo defeat unlocks the special grey door");
        AssertTrue(room.RoomObjects.TryNotifyColoredDoorHit(room.Level.BlockAt(origin), SamusProjectileType.None),
            "Bomb Torizo grey door accepts a post-unlock beam hit");
        room.RoomObjects.Step(room.Level);
        AssertEqual(GreyDoorPhase.Opening, room.RoomObjects.GreyDoorSnapshots.Single().Phase,
            "Bomb Torizo grey door selects its opening list");
        for (int frame = 0; frame < 128 && room.RoomObjects.SlotTable.ActiveCount != 0; frame++)
            room.RoomObjects.Step(room.Level);
        AssertEqual(0, room.RoomObjects.SlotTable.ActiveCount,
            "Bomb Torizo grey door completes its opening and deletes");
        AssertTrue(room.System.HasOpenedDoorBit(0x1b),
            "Bomb Torizo grey door persists its sanitized room argument");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Bomb Torizo grey door avoids both compiled program and draw sources");

        const int width = 16;
        var reopenedLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new RoomTerrainBlock[width * width], new RoomBlockBehavior[width * width], new RoomBlockAppearance[width * width]), RoomLevelData.NothingBeyondGrid());
        var reopened = RoomObjectFixtures.Unbound();
        AssertEqual(1, RoomObjectFixtures.LoadPopulation(ref reopened, new RoomObjectRoom(reopenedLevel, BgArtworkFixtures.For(new SnesVram()), room.System, AreaId.Crateria), RoomObjectPopulationImporter.Read(guarded, population), new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "opened Bomb Torizo grey door reloads");
        // The closed blue list shows the blue cap for one frame and deletes on the next pass.
        reopened.Step(reopenedLevel);
        AssertTrue(reopened.PopulationSlots.Single().Behaviour is BlueDoorCapObject { Phase: BlueDoorCapPhase.Closed },
            "opened Bomb Torizo grey door converts to a blue cap");
        reopened.Step(reopenedLevel);
        AssertEqual(0, reopened.SlotTable.ActiveCount,
            "opened Bomb Torizo grey door deletes after its blue frame");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Bomb Torizo reload avoids both compiled program and draw sources");
    }

    /// <summary>Compares every grey cap's typed programs, Bomb Torizo's included, with their bank-$84 lists.</summary>
    private static void VerifyGreyDoorProgramDefinitions(CartridgeImportAddressSpace rom) =>
        Suite(nameof(RoomObjectProgramsVerification.VerifyGreyDoorPrograms), () => RoomObjectProgramsVerification.VerifyGreyDoorPrograms(rom));


    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    private static void VerifyGreyDoorVisualInstallation(CartridgeImportAddressSpace rom)
    {
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectGreyDoorVisualDirectory);
        GameInstallation installation = copy.Installation;
        RoomObjectGreyDoorVisualFilesConstants.ValidateStock(
            installation.RoomObjectGreyDoorVisualDirectory);
        VerifyGreyDoorStockMapping(rom, installation.LoadRoomObjectGreyDoorVisuals());
        string stockPath = Path.Combine(installation.RoomObjectGreyDoorVisualDirectory,
            RoomObjectGreyDoorVisualFiles.VisualFileName);
        File.WriteAllText(stockPath, "{}");
        AssertThrows<InvalidDataException>(
            () => installation.LoadRoomObjectGreyDoorVisuals(),
            "tampered grey-door stock fails loudly where it is read");
    }
}
