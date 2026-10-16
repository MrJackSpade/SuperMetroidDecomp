using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.Doors.LookupGreyCapVerification;
using static SuperMetroid.Verification.Rooms.Doors.LookupGreyDoorStockVerification;
using SuperMetroid.Verification.Rooms.Plms;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Rooms.Doors;
using SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;

/// <summary>Retail and guarded runtime checks for ordinary and Bomb Torizo grey-door PLM programs and editable draw blocks.</summary>
internal static class GreyDoorPlmDrawDefinitionsVerification
{
    /// <summary>Checks native program words, five door lifecycles, physical draw ownership and installed visual overrides.</summary>
    internal static void VerifyGreyDoorPlmDrawDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyGreyDoorProgramDefinitions), () => VerifyGreyDoorProgramDefinitions(rom));
        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(SnesLittleEndian.Word(GreyDoorPlmDrawDefinitionsRomByte(bus, address), GreyDoorPlmDrawDefinitionsRomByte(bus, address + 1)));

        RoomPlmShotBlockDrawDefinitions.DrawList[] lists =
            GreyDoorPlmDrawDefinitions.All.OrderBy(list => list.Pointer).ToArray();
        RoomPlmGreyDoorVisualEntry[] entries = lists.Select(draw =>
            new RoomPlmGreyDoorVisualEntry(
                GreyDoorPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span[0].LevelWords.Span
                    .SelectToArray(word => new RoomLevelWord(word).VisualWord))).ToArray();
        RoomPlmGreyDoorVisualCatalog stock = RoomPlmGreyDoorVisualCatalog.Stock();
        RoomPlmGreyDoorVisualEntry editedFrame = entries.Single(entry =>
            entry.Id == "grey-right-frame-0");
        ushort originalVisual = editedFrame.Blocks[0];
        editedFrame.Blocks[0] = 0x0053;
        var edited = new RoomPlmGreyDoorVisualCatalog(entries);
        editedFrame.Blocks[0] = 0x0054;
        AssertEqual((ushort)0x0053, edited.GetWord(0xa6d7, 0),
            "grey-door visual catalog copies author data");
        AssertEqual(originalVisual, stock.GetWord(0xa6d7, 0),
            "stock grey-door visual retains native tile choice");
        Suite(nameof(VerifyGreyCapGeometry), () => VerifyGreyCapGeometry(rom));
        Suite(nameof(VerifyGreyCapCollision), () => VerifyGreyCapCollision(rom));
        Suite(nameof(VerifyGreyCapVisuals), () => VerifyGreyCapVisuals(rom));

        var bank84 = new byte[0x8000];
        for (int offset = 0; offset < bank84.Length; offset++)
            bank84[offset] = GreyDoorPlmDrawDefinitionsRomByte(rom, 0x848000 + offset);
        ushort[] headers =
        [
            (ushort)PlmHeaderId.BombTorizoGreyDoor,
            (ushort)PlmHeaderId.GreyDoorFacingLeft,
            (ushort)PlmHeaderId.GreyDoorFacingRight,
            (ushort)PlmHeaderId.GreyDoorFacingUp,
            (ushort)PlmHeaderId.GreyDoorFacingDown,
        ];
        foreach (PlmHeaderId header in headers)
        {
            TestAddressSpace bus = CreateBank84Cartridge(bank84);
            const ushort population = 0x9000;
            SeedRoomPlmPopulation(bus, population,
            [
                unchecked((byte)(ushort)header), SnesLittleEndian.HighByte((ushort)header),
                4, 4,
                header == PlmHeaderId.BombTorizoGreyDoor ? (byte)0x1b : (byte)0,
                header == PlmHeaderId.BombTorizoGreyDoor ? (byte)0x08 : (byte)0,
                0, 0,
            ]);
            const int width = 16;
            const int origin = 4 + 4 * width;
            byte[] blockDefinitions = new byte[0x400 * 8];
            blockDefinitions[(originalVisual & 0x3ff) * 8] = 0xae;
            blockDefinitions[0x53 * 8] = 0x53;
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new ushort[width * width], new byte[width * width], new ushort[width * width]), blockDefinitions);
            var plms = new RoomPlmSystem
            {
                Drawing = { GreyDoorVisuals = header == PlmHeaderId.BombTorizoGreyDoor
                    ? edited : stock },
            };
            var system = new Bank80SystemState();
            var guarded = NewGreyDoorDrawGuard(bus, lists);
            BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
            AssertEqual(1, plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(level, streamer, new SnesVram(), system, AreaId.Crateria), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
                $"resident grey-door header ${(int)header:X4} loads");
            ushort initial = ReadWord(rom, 0x840000 | ((ushort)header + 2));
            ushort firstDraw = ReadWord(rom, 0x840000 | (initial + 12));
            AssertTrue(GreyDoorPlmDrawDefinitions.TryGet(firstDraw, out var selected),
                $"resident grey-door header ${(int)header:X4} selects compiled art");
            plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
            bool vertical = (selected.Runs.Span[0].DirectionAndCount & 0x8000) != 0;
            int stride = vertical ? width : 1;
            for (int block = 0; block < 4; block++)
                AssertEqual(selected.Runs.Span[0].LevelWords.Span[block],
                    level.GetCollisionBlockByIndex(origin + block * stride).LevelWord,
                    $"resident grey-door ${(int)header:X4} draws physical block {block}");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"resident grey-door ${(int)header:X4} avoids draw payload ROM reads");
            if (header == PlmHeaderId.BombTorizoGreyDoor)
            {
                AssertEqual((ushort)0x0053, plms.TilemapUpdates[0].TopRow[0],
                    "edited Bomb Torizo grey cap reaches immediate redraw");
                AssertEqual((ushort)0x0053,
                    level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(origin, 0).TopRow[0],
                    "edited grey cap survives later camera streaming");
                VerifyBombTorizoGreyDoorLifecycle(guarded, new TorizoGreyDoorRoom { Level = level, Streamer = streamer, Plms = plms, System = system }, population, origin, blockDefinitions);
            }
            else
            {
                var enteringDoor = new CartridgeDoorHeader(
                    Pointer: 0,
                    DestinationRoomPointer: 0,
                    Orientation: new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses),
                    PlmBlock: new(4, 4),
                    DestinationScreen: new(0, 0),
                    SamusDistance: 0,
                    SetupCodePointer: DoorSetupCode.None);
                AssertTrue(plms.DoorPlms.TrySpawnDoorClosingPlm(level, enteringDoor,
                        system),
                    $"resident grey door ${(int)header:X4} selects its closing list");
                for (int frame = 0; frame < 20 &&
                     plms.GreyDoorSnapshots.Single().Phase == GreyDoorPhase.Closing; frame++)
                    plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
                AssertEqual(GreyDoorPhase.Locked, plms.GreyDoorSnapshots.Single().Phase,
                    $"resident grey door ${(int)header:X4} returns to locked owner");

                // The default room argument selects the boss-defeated condition. A
                // closed door must flash only after the cartridge event bit changes.
                system.SetBossBits(AreaId.Crateria, BossBits.AreaBoss);
                plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
                AssertEqual(GreyDoorPhase.Flashing, plms.GreyDoorSnapshots.Single().Phase,
                    $"resident grey door ${(int)header:X4} unlocks after boss event");
                AssertTrue(plms.TryNotifyColoredDoorHit(origin,
                        new SamusProjectileTypeWord(0)),
                    $"resident grey door ${(int)header:X4} accepts a beam hit");
                plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
                AssertEqual(GreyDoorPhase.Opening, plms.GreyDoorSnapshots.Single().Phase,
                    $"resident grey door ${(int)header:X4} enters opening list");
                for (int frame = 0; frame < 128 && plms.SlotTable.ActiveCount != 0; frame++)
                    plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
                AssertEqual(0, plms.SlotTable.ActiveCount,
                    $"resident grey door ${(int)header:X4} completes and deletes");
                AssertTrue(system.HasOpenedDoorBit(0),
                    $"resident grey door ${(int)header:X4} persists opened bit");
                AssertEqual(0, guarded.ForbiddenReadAttempts,
                    $"resident grey door ${(int)header:X4} never rereads program or draw data");

                var reopenedLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new ushort[width * width], new byte[width * width], new ushort[width * width]), blockDefinitions);
                BackgroundTilemapStreamer reopenedStreamer =
                    reopenedLevel.CreateBackgroundStreamer();
                var reopened = new RoomPlmSystem { Drawing = { GreyDoorVisuals = stock } };
                AssertEqual(1, reopened.PopulationLoader.LoadRoomPopulation(new PlmRoom(reopenedLevel, reopenedStreamer, new SnesVram(), system, AreaId.Crateria), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
                    $"opened grey door ${(int)header:X4} reloads");
                // The closed blue list shows the blue cap for one frame and deletes on the next pass.
                reopened.Step(reopenedLevel, reopenedStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
                AssertTrue(reopened.PopulationSlots.Single().Animation is BlueDoorPlmAnimation,
                    $"opened grey door ${(int)header:X4} converts to blue cap");
                reopened.Step(reopenedLevel, reopenedStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
                AssertEqual(0, reopened.SlotTable.ActiveCount,
                    $"opened grey door ${(int)header:X4} deletes after its blue frame");
                AssertEqual(0, guarded.ForbiddenReadAttempts,
                    $"opened grey door ${(int)header:X4} avoids program/draw ROM reads");
            }
        }
        editedFrame.Blocks[0] = originalVisual;
        Suite(nameof(VerifySharedDoorClearVisual), () => VerifySharedDoorClearVisual(rom, entries, lists));
        Suite(nameof(VerifyGreyDoorVisualInstallation), () => VerifyGreyDoorVisualInstallation(rom));
        Console.WriteLine(
            "  Grey doors: 420 ordinary and 117 Bomb Torizo program bytes, five close/unlock/open/reload paths, 20 physical draws, and editable visual blocks pass with source reads blocked.");
    }

    /// <summary>Reads retail draw and program bytes only for the comparison oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for grey door PLM draw definitions")]
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
                    address >= (0x840000 | list.Pointer) &&
                    address < (0x840000 | list.Pointer) + GreyDoorPlmDrawDefinitions.DrawListBytes),
                address => $"Resident grey door reread draw payload ${address:X6}.");

    /// <summary>Advances the Bomb-gated door through collection, defeat, hit, opening and persisted blue-cap reload.</summary>
    private static void VerifyBombTorizoGreyDoorLifecycle(CartridgeReadGuard guarded, TorizoGreyDoorRoom room, ushort population, int origin, byte[] blockDefinitions)
    {
        var enteringDoor = new CartridgeDoorHeader(
            Pointer: 0,
            DestinationRoomPointer: 0,
            Orientation: new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses),
            PlmBlock: new(4, 4),
            DestinationScreen: new(0, 0),
            SamusDistance: 0,
            SetupCodePointer: DoorSetupCode.None);
        AssertTrue(room.Plms.DoorPlms.TrySpawnDoorClosingPlm(room.Level, enteringDoor, room.System),
            "Bomb Torizo door selects its Bomb-gated closing list");
        for (int frame = 0; frame < 10; frame++)
            room.Plms.Step(room.Level, room.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(GreyDoorPhase.Closing, room.Plms.GreyDoorSnapshots.Single().Phase,
            "Bomb Torizo closing list loops while Samus has no Bombs");
        for (int frame = 0; frame < 80 &&
             room.Plms.GreyDoorSnapshots.Single().Phase == GreyDoorPhase.Closing; frame++)
            room.Plms.Step(room.Level, room.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }, new PlmFrameInputs { Scrolls = null, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0, CollectedItems = SamusEquipmentFlags.Bombs });
        AssertEqual(GreyDoorPhase.Locked, room.Plms.GreyDoorSnapshots.Single().Phase,
            "collected Bombs let the special closing list rejoin its resident owner");
        room.System.SetBossBits(AreaId.Crateria, BossBits.AreaTorizo);
        room.Plms.Step(room.Level, room.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(GreyDoorPhase.Flashing, room.Plms.GreyDoorSnapshots.Single().Phase,
            "Bomb Torizo defeat unlocks the special grey door");
        AssertTrue(room.Plms.TryNotifyColoredDoorHit(origin, new SamusProjectileTypeWord(0)),
            "Bomb Torizo grey door accepts a post-unlock beam hit");
        room.Plms.Step(room.Level, room.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(GreyDoorPhase.Opening, room.Plms.GreyDoorSnapshots.Single().Phase,
            "Bomb Torizo grey door selects its opening list");
        for (int frame = 0; frame < 128 && room.Plms.SlotTable.ActiveCount != 0; frame++)
            room.Plms.Step(room.Level, room.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0, room.Plms.SlotTable.ActiveCount,
            "Bomb Torizo grey door completes its opening and deletes");
        AssertTrue(room.System.HasOpenedDoorBit(0x1b),
            "Bomb Torizo grey door persists its sanitized room argument");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Bomb Torizo grey door avoids both compiled program and draw sources");

        const int width = 16;
        var reopenedLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new ushort[width * width], new byte[width * width], new ushort[width * width]), blockDefinitions);
        BackgroundTilemapStreamer reopenedStreamer =
            reopenedLevel.CreateBackgroundStreamer();
        var reopened = new RoomPlmSystem();
        AssertEqual(1, reopened.PopulationLoader.LoadRoomPopulation(new PlmRoom(reopenedLevel, reopenedStreamer, new SnesVram(), room.System, AreaId.Crateria), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "opened Bomb Torizo grey door reloads");
        // The closed blue list shows the blue cap for one frame and deletes on the next pass.
        reopened.Step(reopenedLevel, reopenedStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertTrue(reopened.PopulationSlots.Single().Animation is BlueDoorPlmAnimation,
            "opened Bomb Torizo grey door converts to a blue cap");
        reopened.Step(reopenedLevel, reopenedStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
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
        RoomPlmGreyDoorVisualEntry[] entries,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists)
    {
        RoomPlmGreyDoorVisualEntry clear = entries.Single(entry =>
            entry.Id == "clear-right");
        ushort original = clear.Blocks[0];
        clear.Blocks[0] = 0x0054;
        var edited = new RoomPlmGreyDoorVisualCatalog(entries);
        clear.Blocks[0] = original;

        const int width = 16;
        const int origin = 4 + 4 * width;
        byte[] blockDefinitions = new byte[0x400 * 8];
        blockDefinitions[0x54 * 8] = 0x54;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new ushort[width * width], new byte[width * width], new ushort[width * width]), blockDefinitions);
        var plms = new RoomPlmSystem { Drawing = { GreyDoorVisuals = edited } };
        AssertTrue(plms.DoorPlms.TrySpawnBlueDoorOpening(level, origin,
                RoomBlockBehaviorValues.BlueDoorFacingRight,
                new SamusProjectileTypeWord(0)),
            "shared clear visual test allocates a real blue-door opening actor");
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var guarded = NewGreyDoorDrawGuard(rom, lists);
        int frame = 0;
        while (frame++ < 200 &&
            level.GetCollisionBlockByIndex(origin).LevelWord != 0x0482)
            plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertTrue(frame <= 200,
            "blue-door opening reaches the shared right-facing clear draw");
        AssertEqual((ushort)0x0482,
            level.GetCollisionBlockByIndex(origin).LevelWord,
            "shared clear art leaves the native physical air word intact");
        AssertEqual((ushort)0x0054,
            level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(origin, 0).TopRow[0],
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
            ushort stock = installed.GetWord(0xa6d7, 0);
            string stockPath = Path.Combine(installation.RoomPlmGreyDoorVisualDirectory,
                RoomPlmGreyDoorVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted grey-door JSON is empty.");
            JsonNode frame = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "grey-right-frame-0");
            frame.Required("blocks")[0] = 0x0053;
            Directory.CreateDirectory(
                installation.RoomPlmGreyDoorVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmGreyDoorVisualOverrideDirectory,
                RoomPlmGreyDoorVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0053,
                installation.LoadRoomPlmGreyDoorVisuals().GetWord(0xa6d7, 0),
                "installed grey-door override changes selected frame");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmGreyDoorVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmGreyDoorVisualFiles.Load(refreshed,
                    installation.RoomPlmGreyDoorVisualOverrideDirectory)
                    .GetWord(0xa6d7, 0),
                "grey-door override survives stock replacement");

            frame.Required("blocks")[0] = 0xf053;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmGreyDoorVisuals(),
                "invalid grey-door override fails loudly");
            frame.Required("blocks")[0] = stock;
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
