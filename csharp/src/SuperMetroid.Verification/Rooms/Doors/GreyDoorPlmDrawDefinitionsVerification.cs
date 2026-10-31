using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.LookupGreyCapVerification;
using static SuperMetroid.Verification.Rooms.Doors.LookupGreyDoorStockVerification;
using SuperMetroid.Verification.Rooms.Plms;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
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
internal static class GreyDoorPlmDrawDefinitionsVerification
{
    /// <summary>Checks native program words, five door lifecycles, physical draw ownership and installed visual overrides.</summary>
    internal static void VerifyGreyDoorPlmDrawDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyGreyDoorProgramDefinitions), () => VerifyGreyDoorProgramDefinitions(rom));
        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            LittleEndianBytes.Word(GreyDoorPlmDrawDefinitionsRomByte(bus, address), GreyDoorPlmDrawDefinitionsRomByte(bus, address + 1));

        RoomPlmShotBlockDrawDefinitions.DrawList[] lists =
            GreyDoorCaps.All.OrderBy(list => PlmDrawAddressCodec.Of(list.Draw)).ToArray();
        RoomPlmVisualEntry[] entries = lists.Select(draw =>
            new RoomPlmVisualEntry(
                GreyDoorCaps.VisualId(draw.Draw),
                draw.Runs.Span[0].Blocks.Span
                    .SelectToArray(block => block.Appearance))).ToArray();
        InstalledRoomPlmGreyDoorVisuals stock = InstalledRoomPlmGreyDoorVisuals.Stock();
        RoomPlmVisualEntry editedFrame = entries.Single(entry =>
            entry.Id == "grey-right-frame-0");
        RoomBlockAppearance originalVisual = editedFrame.Blocks[0];
        editedFrame.Blocks[0] = RoomBlockAppearance.Of(0x053);
        var edited = new InstalledRoomPlmGreyDoorVisuals(entries);
        editedFrame.Blocks[0] = RoomBlockAppearance.Of(0x054);
        AssertEqual(RoomBlockAppearance.Of(0x053), edited.GetAppearance(NativePlmDraws.At<GreyDoorCaps.Draw>(0xa6d7), 0),
            "grey-door visual catalog copies author data");
        AssertEqual(originalVisual, stock.GetAppearance(NativePlmDraws.At<GreyDoorCaps.Draw>(0xa6d7), 0),
            "stock grey-door visual retains native tile choice");
        Suite(nameof(VerifyGreyCapGeometry), () => VerifyGreyCapGeometry(rom));
        Suite(nameof(VerifyGreyCapCollision), () => VerifyGreyCapCollision(rom));
        Suite(nameof(VerifyGreyCapVisuals), () => VerifyGreyCapVisuals(rom));

        var bank84 = new byte[0x8000];
        for (int offset = 0; offset < bank84.Length; offset++)
            bank84[offset] = GreyDoorPlmDrawDefinitionsRomByte(rom, 0x848000 + offset);
        PlmHeaderId[] headers =
        [
            PlmHeaderId.BombTorizoGreyDoor,
            PlmHeaderId.GreyDoorFacingLeft,
            PlmHeaderId.GreyDoorFacingRight,
            PlmHeaderId.GreyDoorFacingUp,
            PlmHeaderId.GreyDoorFacingDown,
        ];
        foreach (PlmHeaderId header in headers)
        {
            TestAddressSpace bus = CreateBank84Cartridge(bank84);
            const ushort population = 0x9000;
            SeedRoomPlmPopulation(bus, population,
            [
                unchecked((byte)PlmNativeWords.Word(header)), LittleEndianBytes.HighByte(PlmNativeWords.Word(header)),
                4, 4,
                header == PlmHeaderId.BombTorizoGreyDoor ? (byte)0x1b : (byte)0,
                header == PlmHeaderId.BombTorizoGreyDoor ? (byte)0x08 : (byte)0,
                0, 0,
            ]);
            const int width = 16;
            const int origin = 4 + 4 * width;
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new RoomTerrainBlock[width * width], new RoomBlockBehavior[width * width], new RoomBlockAppearance[width * width]));
            var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmGreyDoorVisuals = header == PlmHeaderId.BombTorizoGreyDoor
                    ? edited : stock });
            var system = new SharedGameState();
            var guarded = NewGreyDoorDrawGuard(bus, lists);
            AssertEqual(1, PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, BgArtworkFixtures.For(new SnesVram()), system, AreaId.Crateria), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation)), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
                $"resident grey-door header {header} loads");
            ushort initial = ReadWord(rom, 0x840000 | (PlmNativeWords.Word(header) + 2));
            ushort firstDraw = ReadWord(rom, 0x840000 | (initial + 12));
            AssertTrue(GreyDoorCaps.TryGet(NativePlmDraws.At<GreyDoorCaps.Draw>(firstDraw), out var selected),
                $"resident grey-door header {header} selects compiled art");
            plms.Step(level);
            bool vertical = selected.Runs.Span[0].Direction == PlmRunDirection.Vertical;
            int stride = vertical ? width : 1;
            for (int block = 0; block < 4; block++)
                AssertEqual(selected.Runs.Span[0].Blocks.Span[block],
                    level.GetCollisionBlockByIndex(origin + block * stride).Terrain,
                    $"resident grey-door {header} draws physical block {block}");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"resident grey-door {header} avoids draw payload ROM reads");
            if (header == PlmHeaderId.BombTorizoGreyDoor)
            {
                AssertEqual(RoomBlockAppearance.Of(0x053),
                    level.DisplayedAppearance(origin),
                    "edited grey cap is shown");
                VerifyBombTorizoGreyDoorLifecycle(guarded, new TorizoGreyDoorRoom { Level = level, Plms = plms, System = system }, population, origin);
            }
            else
            {
                var enteringDoor = new DoorDefinition(
                    Name: "fixture door",
                    DestinationRoom: FixtureRoomHeaders.UnloadedDestination,
                    Orientation: new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses),
                    PlmBlock: new(4, 4),
                    DestinationScreen: new(0, 0),
                    SamusSpeed: DoorSamusSpeed.None,
                    SetupCode: DoorSetupCode.None);
                AssertTrue(plms.DoorPlms.TrySpawnDoorClosingPlm(level, enteringDoor,
                        system),
                    $"resident grey door {header} selects its closing list");
                for (int frame = 0; frame < 20 &&
                     plms.GreyDoorSnapshots.Single().Phase == GreyDoorPhase.Closing; frame++)
                    plms.Step(level);
                AssertEqual(GreyDoorPhase.Locked, plms.GreyDoorSnapshots.Single().Phase,
                    $"resident grey door {header} returns to locked owner");

                // The default room argument selects the boss-defeated condition. A
                // closed door must flash only after the cartridge event bit changes.
                system.SetBossBits(AreaId.Crateria, BossBits.AreaBoss);
                plms.Step(level);
                AssertEqual(GreyDoorPhase.Flashing, plms.GreyDoorSnapshots.Single().Phase,
                    $"resident grey door {header} unlocks after boss event");
                AssertTrue(plms.TryNotifyColoredDoorHit(origin,
                        SamusProjectileType.None),
                    $"resident grey door {header} accepts a beam hit");
                plms.Step(level);
                AssertEqual(GreyDoorPhase.Opening, plms.GreyDoorSnapshots.Single().Phase,
                    $"resident grey door {header} enters opening list");
                for (int frame = 0; frame < 128 && plms.SlotTable.ActiveCount != 0; frame++)
                    plms.Step(level);
                AssertEqual(0, plms.SlotTable.ActiveCount,
                    $"resident grey door {header} completes and deletes");
                AssertTrue(system.HasOpenedDoorBit(0),
                    $"resident grey door {header} persists opened bit");
                AssertEqual(0, guarded.ForbiddenReadAttempts,
                    $"resident grey door {header} never rereads program or draw data");

                var reopenedLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new RoomTerrainBlock[width * width], new RoomBlockBehavior[width * width], new RoomBlockAppearance[width * width]));
                var reopened = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmGreyDoorVisuals = stock });
                AssertEqual(1, PlmFixtures.LoadPopulation(ref reopened, new PlmRoom(reopenedLevel, BgArtworkFixtures.For(new SnesVram()), system, AreaId.Crateria), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation)), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
                    $"opened grey door {header} reloads");
                // The closed blue list shows the blue cap for one frame and deletes on the next pass.
                reopened.Step(reopenedLevel);
                AssertTrue(reopened.PopulationSlots.Single().Animation is BlueDoorPlmAnimation,
                    $"opened grey door {header} converts to blue cap");
                reopened.Step(reopenedLevel);
                AssertEqual(0, reopened.SlotTable.ActiveCount,
                    $"opened grey door {header} deletes after its blue frame");
                AssertEqual(0, guarded.ForbiddenReadAttempts,
                    $"opened grey door {header} avoids program/draw ROM reads");
            }
        }
        editedFrame.Blocks[0] = originalVisual;
        Suite(nameof(VerifySharedDoorClearVisual), () => VerifySharedDoorClearVisual(rom, entries, lists));
        Suite(nameof(VerifyGreyDoorVisualInstallation), () => VerifyGreyDoorVisualInstallation(rom));
        Console.WriteLine(
            "  Grey doors: 420 ordinary and 117 Bomb Torizo program bytes, five close/unlock/open/reload paths, 20 physical draws, and editable visual blocks pass with source reads blocked.");
    }

    /// <summary>Reads retail draw and program bytes only for the comparison oracle.</summary>
    private static byte GreyDoorPlmDrawDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Blocks compiled ordinary, Bomb Torizo and draw-list byte ranges during resident-door playback.</summary>
    private static CartridgeReadGuard NewGreyDoorDrawGuard(
        ISnesAddressSpace source,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x84be59 and <= 0x84bffc,
                address => $"Resident grey door reread compiled instruction ${address:X6}.")
            .Forbid(address => address is >= 0x84ba4c and <= 0x84ba6e or >= 0x84ba7f and <= 0x84bad0,
                address => $"Bomb Torizo grey door reread compiled instruction ${address:X6}.")
            .Forbid(address => lists.Any(list =>
                    address >= (0x840000 | PlmDrawAddressCodec.Of(list.Draw)) &&
                    address < (0x840000 | PlmDrawAddressCodec.Of(list.Draw)) + 12),
                address => $"Resident grey door reread draw payload ${address:X6}.");

    /// <summary>Advances the Bomb-gated door through collection, defeat, hit, opening and persisted blue-cap reload.</summary>
    private static void VerifyBombTorizoGreyDoorLifecycle(CartridgeReadGuard guarded, TorizoGreyDoorRoom room, ushort population, int origin)
    {
        var enteringDoor = new DoorDefinition(
            Name: "fixture door",
            DestinationRoom: FixtureRoomHeaders.UnloadedDestination,
            Orientation: new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses),
            PlmBlock: new(4, 4),
            DestinationScreen: new(0, 0),
            SamusSpeed: DoorSamusSpeed.None,
            SetupCode: DoorSetupCode.None);
        AssertTrue(room.Plms.DoorPlms.TrySpawnDoorClosingPlm(room.Level, enteringDoor, room.System),
            "Bomb Torizo door selects its Bomb-gated closing list");
        for (int frame = 0; frame < 10; frame++)
            room.Plms.Step(room.Level);
        AssertEqual(GreyDoorPhase.Closing, room.Plms.GreyDoorSnapshots.Single().Phase,
            "Bomb Torizo closing list loops while Samus has no Bombs");
        for (int frame = 0; frame < 80 &&
             room.Plms.GreyDoorSnapshots.Single().Phase == GreyDoorPhase.Closing; frame++)
            room.Plms.Step(room.Level, new PlmFrameInputs { Scrolls = null, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0, CollectedItems = SamusEquipmentFlags.Bombs });
        AssertEqual(GreyDoorPhase.Locked, room.Plms.GreyDoorSnapshots.Single().Phase,
            "collected Bombs let the special closing list rejoin its resident owner");
        room.System.SetBossBits(AreaId.Crateria, BossBits.AreaTorizo);
        room.Plms.Step(room.Level);
        AssertEqual(GreyDoorPhase.Flashing, room.Plms.GreyDoorSnapshots.Single().Phase,
            "Bomb Torizo defeat unlocks the special grey door");
        AssertTrue(room.Plms.TryNotifyColoredDoorHit(origin, SamusProjectileType.None),
            "Bomb Torizo grey door accepts a post-unlock beam hit");
        room.Plms.Step(room.Level);
        AssertEqual(GreyDoorPhase.Opening, room.Plms.GreyDoorSnapshots.Single().Phase,
            "Bomb Torizo grey door selects its opening list");
        for (int frame = 0; frame < 128 && room.Plms.SlotTable.ActiveCount != 0; frame++)
            room.Plms.Step(room.Level);
        AssertEqual(0, room.Plms.SlotTable.ActiveCount,
            "Bomb Torizo grey door completes its opening and deletes");
        AssertTrue(room.System.HasOpenedDoorBit(0x1b),
            "Bomb Torizo grey door persists its sanitized room argument");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Bomb Torizo grey door avoids both compiled program and draw sources");

        const int width = 16;
        var reopenedLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new RoomTerrainBlock[width * width], new RoomBlockBehavior[width * width], new RoomBlockAppearance[width * width]));
        var reopened = PlmFixtures.Unbound();
        AssertEqual(1, PlmFixtures.LoadPopulation(ref reopened, new PlmRoom(reopenedLevel, BgArtworkFixtures.For(new SnesVram()), room.System, AreaId.Crateria), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation)), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "opened Bomb Torizo grey door reloads");
        // The closed blue list shows the blue cap for one frame and deletes on the next pass.
        reopened.Step(reopenedLevel);
        AssertTrue(reopened.PopulationSlots.Single().Animation is BlueDoorPlmAnimation,
            "opened Bomb Torizo grey door converts to a blue cap");
        reopened.Step(reopenedLevel);
        AssertEqual(0, reopened.SlotTable.ActiveCount,
            "opened Bomb Torizo grey door deletes after its blue frame");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Bomb Torizo reload avoids both compiled program and draw sources");
    }

    /// <summary>Compares every grey cap's typed programs, Bomb Torizo's included, with their bank-$84 lists.</summary>
    private static void VerifyGreyDoorProgramDefinitions(SuperMetroidAddressSpace rom) =>
        Suite(nameof(RoomPlmProgramsVerification.VerifyGreyDoorPrograms), () => RoomPlmProgramsVerification.VerifyGreyDoorPrograms(rom));


    /// <summary>Checks that an edited clear tile reaches shared blue-door streaming without changing physical door behavior.</summary>
    private static void VerifySharedDoorClearVisual(
        SuperMetroidAddressSpace rom,
        RoomPlmVisualEntry[] entries,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists)
    {
        RoomPlmVisualEntry clear = entries.Single(entry =>
            entry.Id == "clear-right");
        RoomBlockAppearance original = clear.Blocks[0];
        clear.Blocks[0] = RoomBlockAppearance.Of(0x054);
        var edited = new InstalledRoomPlmGreyDoorVisuals(entries);
        clear.Blocks[0] = original;

        const int width = 16;
        const int origin = 4 + 4 * width;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new RoomTerrainBlock[width * width], new RoomBlockBehavior[width * width], new RoomBlockAppearance[width * width]));
        var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmGreyDoorVisuals = edited });
        AssertTrue(plms.DoorPlms.TrySpawnBlueDoorOpening(level, origin,
                RoomBlockBehaviorValues.BlueDoorFacingRight,
                SamusProjectileType.None),
            "shared clear visual test allocates a real blue-door opening actor");
        var guarded = NewGreyDoorDrawGuard(rom, lists);
        int frame = 0;
        while (frame++ < 200 &&
            level.GetCollisionBlockByIndex(origin).Terrain != RoomTerrainBlock.Of(0x082, BlockMirroring.Horizontal, RoomCollisionType.Air))
            plms.Step(level);
        AssertTrue(frame <= 200,
            "blue-door opening reaches the shared right-facing clear draw");
        AssertEqual(RoomTerrainBlock.Of(0x082, BlockMirroring.Horizontal, RoomCollisionType.Air),
            level.GetCollisionBlockByIndex(origin).Terrain,
            "shared clear art leaves the native physical air word intact");
        AssertEqual(RoomBlockAppearance.Of(0x054),
            level.DisplayedAppearance(origin),
            "edited shared clear tile reaches blue-door camera streaming");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "blue-door shared clear draw does not reread bank-$84 payload bytes");
    }

    /// <summary>Extracts stock grey-door art, applies an override and checks refresh persistence and tamper rejection.</summary>
    private static void VerifyGreyDoorVisualInstallation(SuperMetroidAddressSpace rom)
    {
        using var testRootScratch = new TestScratchDirectory("grey-door-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmGreyDoorVisualFiles.Extract(rom,
                installation.RoomPlmGreyDoorVisualDirectory, SupportedCartridge.Sha256);
            RoomPlmGreyDoorVisualFiles.ValidateStock(
                installation.RoomPlmGreyDoorVisualDirectory);
            var installed = installation.LoadRoomPlmGreyDoorVisuals();
            VerifyGreyDoorStockMapping(rom, installed);
            RoomBlockAppearance stock = installed.GetAppearance(NativePlmDraws.At<GreyDoorCaps.Draw>(0xa6d7), 0);
            string stockPath = Path.Combine(installation.RoomPlmGreyDoorVisualDirectory,
                RoomPlmGreyDoorVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted grey-door JSON is empty.");
            JsonNode frame = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "grey-right-frame-0");
            frame.Required("blocks")[0] = PlmVisualJson.Appearance(0x053);
            Directory.CreateDirectory(
                installation.RoomPlmGreyDoorVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmGreyDoorVisualOverrideDirectory,
                RoomPlmGreyDoorVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual(RoomBlockAppearance.Of(0x053),
                installation.LoadRoomPlmGreyDoorVisuals().GetAppearance(NativePlmDraws.At<GreyDoorCaps.Draw>(0xa6d7), 0),
                "installed grey-door override changes selected frame");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmGreyDoorVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual(RoomBlockAppearance.Of(0x053),
                RoomPlmGreyDoorVisualFiles.Load(refreshed,
                    installation.RoomPlmGreyDoorVisualOverrideDirectory)
                    .GetAppearance(NativePlmDraws.At<GreyDoorCaps.Draw>(0xa6d7), 0),
                "grey-door override survives stock replacement");

            frame.Required("blocks")[0] = 0xf053;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmGreyDoorVisuals(),
                "invalid grey-door override fails loudly");
            frame.Required("blocks")[0] = PlmVisualJson.Appearance(stock);
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmGreyDoorVisuals(),
                "tampered grey-door stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }
}
