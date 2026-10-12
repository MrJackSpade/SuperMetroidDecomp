using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Spatial;
using static AssertionsVerification;
using static EyeDoorPlmsVerification;
using static LookupEyeDoorLayoutsVerification;
using static LookupEyeDoorProgramVerification;
using static LookupEyeDoorStockVerification;
using static LookupEyeDoorVisualIdsVerification;
using static SuiteTimingVerification;
using static SyntheticRoomCartridgeFixturesVerification;

/// <summary>Represents eye door plm draw definitions verification.</summary>
internal static class EyeDoorPlmDrawDefinitionsVerification
{
    /// <summary>verify eye door plm draw definitions.</summary>
    internal static void VerifyEyeDoorPlmDrawDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyEyeDoorVisualIds), () => VerifyEyeDoorVisualIds());
        Suite(nameof(VerifyEyeDoorProgramDefinitions), () => VerifyEyeDoorProgramDefinitions(rom));
        Suite(nameof(VerifyEyeDoorLayoutGeometry), () => VerifyEyeDoorLayoutGeometry(rom));
        Suite(nameof(VerifyEyeDoorLayoutCollision), () => VerifyEyeDoorLayoutCollision(rom));
        Suite(nameof(VerifyEyeDoorLayoutVisuals), () => VerifyEyeDoorLayoutVisuals(rom));
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists =
            EyeDoorPlmDrawDefinitions.All.OrderBy(list => list.Pointer).ToArray();
        RoomPlmEyeDoorVisualEntry[] entries = EyeDoorPlmDrawDefinitions.Editable.Select(draw =>
            new RoomPlmEyeDoorVisualEntry(
                EyeDoorPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span[0].LevelWords.Span.ToArray()
                    .Select(word => new RoomLevelWord(word).VisualWord).ToArray())).ToArray();
        RoomPlmEyeDoorVisualEntry rightEye = entries.Single(entry =>
            entry.Id == "right-eye-frame-0");
        ushort stockWord = rightEye.Blocks[0];
        rightEye.Blocks[0] = 0x0055;
        var edited = new RoomPlmEyeDoorVisualCatalog(entries);
        rightEye.Blocks[0] = 0x0056;
        AssertEqual((ushort)0x0055, edited.GetWord(0x9c5b, 0),
            "eye-door catalog copies author data");
        rightEye.Blocks[0] = stockWord;
        AssertEqual(24, lists.Length,
            "mirrored eye, middle and bottom components include both four-block opening clears");
        AssertEqual(23, entries.Length, "the mirrored clear preserves the existing authored visual identities");
        for (int block = 0; block < 4; block++)
            AssertEqual((ushort)(edited.GetWord(EyeDoorPlmDrawDefinitions.VisualSource(
                EyeDoorPlmDrawDefinitions.MirroredOpeningClear), block) ^ (ushort)LevelBlockFlipFlags.Horizontal),
                edited.GetWord(EyeDoorPlmDrawDefinitions.MirroredOpeningClear, block),
                "left opening clear mirrors its authored appearance without changing the override schema");
        Suite(nameof(VerifyEyeDoorNativeDrawPath), () => VerifyEyeDoorNativeDrawPath(EyeDoorOrientation.Left, lists, null));
        Suite(nameof(VerifyEyeDoorNativeDrawPath), () => VerifyEyeDoorNativeDrawPath(EyeDoorOrientation.Right, lists, edited));
        Suite(nameof(VerifyEyeDoorRetailProgramPath), () => VerifyEyeDoorRetailProgramPath(rom, EyeDoorOrientation.Left, lists));
        Suite(nameof(VerifyEyeDoorRetailProgramPath), () => VerifyEyeDoorRetailProgramPath(rom, EyeDoorOrientation.Right, lists));
        Suite(nameof(VerifyEyeDoorVisualInstallation), () => VerifyEyeDoorVisualInstallation(rom));
        Console.WriteLine(
            "  Eye doors: 622 compiled instruction bytes, guarded mirrored lifecycles, 24 physical draws and 23 compatible authored identities preserve collision.");
    }

    /// <summary>verify eye door program definitions.</summary>
    private static void VerifyEyeDoorProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyEyeDoorProgramControl), () => VerifyEyeDoorProgramControl(rom));
        Suite(nameof(VerifyEyeDoorProgramDuration), () => VerifyEyeDoorProgramDuration(rom));
        Suite(nameof(VerifyEyeDoorProgramDraw), () => VerifyEyeDoorProgramDraw(rom));
        Suite(nameof(VerifyEyeDoorProgramTarget), () => VerifyEyeDoorProgramTarget(rom));
        Suite(nameof(VerifyEyeDoorProgramCallback), () => VerifyEyeDoorProgramCallback(rom));
        Suite(nameof(VerifyEyeDoorProgramColumns), () => VerifyEyeDoorProgramColumns(rom));
        Suite(nameof(VerifyEyeDoorProgramRows), () => VerifyEyeDoorProgramRows(rom));
        Suite(nameof(VerifyEyeDoorProgramAttack), () => VerifyEyeDoorProgramAttack(rom));
        Suite(nameof(VerifyEyeDoorProgramSweat), () => VerifyEyeDoorProgramSweat(rom));
        Suite(nameof(VerifyEyeDoorProgramSound), () => VerifyEyeDoorProgramSound(rom));
        Suite(nameof(VerifyEyeDoorProgramHitCount), () => VerifyEyeDoorProgramHitCount(rom));
        Suite(nameof(VerifyEyeDoorProgramLoopCount), () => VerifyEyeDoorProgramLoopCount(rom));
    }

    /// <summary>verify eye door native draw path.</summary>
    private static void VerifyEyeDoorNativeDrawPath(
        EyeDoorOrientation orientation,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists,
        RoomPlmEyeDoorVisualCatalog? visuals)
    {
        const int width = 16;
        var bus = new TestAddressSpace();
        SeedEyeDoorFixtureRom(bus, orientation);
        ushort[] pointers = orientation == EyeDoorOrientation.Left
            ? [0x9c03, 0x9c2b, 0x9c3d]
            : [0x9c5b, 0x9c83, 0x9c95];
        SeedPlmProgramWords(bus, 0x041a, pointers[0]);
        SeedPlmProgramWords(bus, 0x050e, pointers[1]);
        SeedPlmProgramWords(bus, 0x060e, pointers[2]);

        byte[] blockDefinitions = new byte[0x400 * 8];
        blockDefinitions[0x55 * 8] = 0x55;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new ushort[width * width], new byte[width * width], new ushort[width * width]), blockDefinitions);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var plms = new RoomPlmSystem { Drawing = { EyeDoorVisuals = visuals } };
        var guarded = NewEyeDoorDrawGuard(bus, lists);
        AssertEqual(3, plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(guarded, level, streamer, new SnesVram(), new Bank80SystemState(), AreaId.Brinstar), RoomPlmPopulationImporter.Read(guarded, 0x9000), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Enemies = new FixturePlmEnemyEffects { EyeDoorProjectile = _ => { } } }),
            $"{orientation} eye-door three-component population loads");
        plms.Step(guarded, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        int[] origins = [4 * width + 7, 6 * width + 7, 8 * width + 7];
        for (int component = 0; component < pointers.Length; component++)
        {
            AssertTrue(EyeDoorPlmDrawDefinitions.TryGet(pointers[component], out var list),
                $"{orientation} component {component} selects compiled draw data");
            RoomPlmShotBlockDrawDefinitions.Run run = list.Runs.Span[0];
            int stride = (run.DirectionAndCount & 0x8000) != 0 ? width : 1;
            for (int block = 0; block < run.LevelWords.Length; block++)
                AssertEqual(run.LevelWords.Span[block],
                    level.GetCollisionBlockByIndex(origins[component] + block * stride).LevelWord,
                    $"{orientation} component {component} draws physical block {block}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"{orientation} eye-door first draws avoid bank-$84 payload reads");
        if (orientation == EyeDoorOrientation.Right)
        {
            AssertTrue(plms.TilemapUpdates.Any(update => update.TopRow[0] == 0x0055),
                "edited right eye reaches the immediate PLM tile update");
            AssertEqual((ushort)0x0055,
                level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(origins[0], 0)
                    .TopRow[0],
                "edited right eye survives later camera streaming");
        }
    }

    /// <summary>verify eye door retail program path.</summary>
    private static void VerifyEyeDoorRetailProgramPath(
        SuperMetroidAddressSpace rom,
        EyeDoorOrientation orientation,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists)
    {
        const int width = 16;
        const byte eyeX = 7;
        const byte eyeY = 4;
        const ushort doorBit = 5;
        ushort[] headers = orientation == EyeDoorOrientation.Left
            ? [(ushort)PlmHeaderId.EyeDoorEyeFacingLeft,
                (ushort)PlmHeaderId.EyeDoorFacingLeft,
                (ushort)PlmHeaderId.EyeDoorBottomFacingLeft]
            : [(ushort)PlmHeaderId.EyeDoorEyeFacingRight,
                (ushort)PlmHeaderId.EyeDoorFacingRight,
                (ushort)PlmHeaderId.EyeDoorBottomFacingRight];
        var bank84 = new byte[0x8000];
        for (int offset = 0; offset < bank84.Length; offset++)
            bank84[offset] = EyeDoorRetailProgramPathRomByte(rom, 0x848000 + offset);
        TestAddressSpace bus = CreateBank84Cartridge(bank84);
        const ushort population = 0x9000;
        SeedRoomPlmPopulation(bus, population,
        [
            unchecked((byte)headers[0]), SnesLittleEndian.HighByte(headers[0]), eyeX, eyeY,
            unchecked((byte)doorBit), 0,
            unchecked((byte)headers[1]), SnesLittleEndian.HighByte(headers[1]), eyeX, eyeY + 2,
            unchecked((byte)doorBit), 0,
            unchecked((byte)headers[2]), SnesLittleEndian.HighByte(headers[2]), eyeX, eyeY + 4,
            unchecked((byte)doorBit), 0,
            0, 0,
        ]);
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new ushort[width * width], new byte[width * width], new ushort[width * width]), new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var samus = new SamusState
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(eyeX * 16 + 8, eyeY * 16 + 8)),
        };
        var requests = new List<EyeDoorProjectileRequest>();
        var system = new Bank80SystemState();
        var plms = new RoomPlmSystem();
        var guarded = NewEyeDoorDrawGuard(bus, lists);
        AssertEqual(3, plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(guarded, level, streamer, new SnesVram(), system, AreaId.Brinstar), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Enemies = new FixturePlmEnemyEffects { EyeDoorProjectile = requests.Add } }),
            $"{orientation} retail eye-door program loads all three components");

        int eyeBlock = eyeY * width + eyeX;
        for (int frame = 0; frame < 1024 &&
             plms.EyeDoorSnapshots.Single(door => door.Component == EyeDoorComponent.Eye)
                 .PreInstruction != (ushort)EyeDoorPreInstruction.MissileHit; frame++)
            plms.Step(guarded, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual((ushort)EyeDoorPreInstruction.MissileHit,
            plms.EyeDoorSnapshots.Single(door => door.Component == EyeDoorComponent.Eye)
                .PreInstruction,
            $"{orientation} retail eye-door program arms its missile pre-instruction");
        AssertTrue(plms.TryNotifyColoredDoorHit(eyeBlock,
                new SamusProjectileTypeWord(0x0200)),
            $"{orientation} retail eye door accepts a Super Missile collision");
        plms.Step(guarded, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        for (int frame = 0; frame < 1024 && plms.SlotTable.ActiveCount != 0; frame++)
            plms.Step(guarded, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertTrue(system.HasOpenedDoorBit(doorBit),
            $"{orientation} retail eye-door opening persists the door bit");
        AssertEqual(0, plms.SlotTable.ActiveCount,
            $"{orientation} retail eye and passive components complete and delete");
        RoomCollisionBlock cap = level.GetCollisionBlockByIndex(eyeBlock - width);
        AssertEqual(RoomCollisionType.ShootableBlock, cap.CollisionType,
            $"{orientation} retail eye opening constructs the blue cap");
        AssertEqual(orientation == EyeDoorOrientation.Left
                ? RoomBlockBehaviorValues.BlueDoorFacingLeft.Value
                : RoomBlockBehaviorValues.BlueDoorFacingRight.Value,
            cap.Behavior,
            $"{orientation} retail eye opening selects the blue-cap orientation");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"{orientation} retail eye-door lifecycle avoids instruction and draw ROM reads");

        var reopenedLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new ushort[width * width], new byte[width * width], new ushort[width * width]), new byte[0x400 * 8]);
        BackgroundTilemapStreamer reopenedStreamer =
            reopenedLevel.CreateBackgroundStreamer();
        var reopened = new RoomPlmSystem();
        AssertEqual(3, reopened.PopulationLoader.LoadRoomPopulation(new PlmRoom(guarded, reopenedLevel, reopenedStreamer, new SnesVram(), system, AreaId.Brinstar), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Enemies = new FixturePlmEnemyEffects { EyeDoorProjectile = requests.Add } }),
            $"{orientation} opened eye-door room reloads three components");
        for (int frame = 0; frame < 16 && reopened.SlotTable.ActiveCount != 0; frame++)
            reopened.Step(guarded, reopenedLevel, reopenedStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0, reopened.SlotTable.ActiveCount,
            $"{orientation} opened eye-door reload converts to blue and removes passive parts");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"{orientation} opened eye-door reload avoids instruction and draw ROM reads");
    }

    /// <summary>eye door retail program path rom byte.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for eye door retail program path")]
    private static byte EyeDoorRetailProgramPathRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>verify eye door visual installation.</summary>
    private static void VerifyEyeDoorVisualInstallation(SuperMetroidAddressSpace rom)
    {
        using var testRootScratch = new TestScratchDirectory("eye-door-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmEyeDoorVisualFiles.Extract(rom,
                installation.RoomPlmEyeDoorVisualDirectory, SupportedCartridge.Sha256);
            RoomPlmEyeDoorVisualFiles.ValidateStock(
                installation.RoomPlmEyeDoorVisualDirectory);
            VerifyEyeDoorStockMapping(rom, installation.LoadRoomPlmEyeDoorVisuals());
            string stockPath = Path.Combine(installation.RoomPlmEyeDoorVisualDirectory,
                RoomPlmEyeDoorVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted eye-door JSON is empty.");
            JsonNode frame = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() == "right-eye-frame-0")!;
            frame["blocks"]![0] = 0x0055;
            Directory.CreateDirectory(
                installation.RoomPlmEyeDoorVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmEyeDoorVisualOverrideDirectory,
                RoomPlmEyeDoorVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0055,
                installation.LoadRoomPlmEyeDoorVisuals().GetWord(0x9c5b, 0),
                "installed eye-door override changes the selected frame");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmEyeDoorVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0055,
                RoomPlmEyeDoorVisualFiles.Load(refreshed,
                    installation.RoomPlmEyeDoorVisualOverrideDirectory)
                    .GetWord(0x9c5b, 0),
                "eye-door override survives stock replacement");

            frame["blocks"]![0] = 0xf055;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmEyeDoorVisuals(),
                "invalid eye-door override fails loudly");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmEyeDoorVisuals(),
                "tampered eye-door stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    /// <summary>new eye door draw guard.</summary>
    private static MutableCartridgeReadGuard NewEyeDoorDrawGuard(
        ISnesAddressSpace source, RoomPlmShotBlockDrawDefinitions.DrawList[] lists)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => address is >= 0x84d81e and <= 0x84da8b,
                address => $"Eye door reread compiled instruction ${address:X6}.")
            .Forbid(address => lists.Any(list =>
                {
                    int first = 0x840000 | list.Pointer;
                    int length = 4 + list.Runs.Span[0].LevelWords.Length * 2;
                    return address >= first && address < first + length;
                }),
                address => $"Eye door reread compiled draw payload ${address:X6}.");
        return guard;
    }
}
