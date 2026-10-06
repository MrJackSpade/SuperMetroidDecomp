using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEyeDoorPlmDrawDefinitions(SuperMetroidAddressSpace rom)
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
        WriteWord(bus, 0x84e01a, pointers[0]);
        WriteWord(bus, 0x84e10e, pointers[1]);
        WriteWord(bus, 0x84e20e, pointers[2]);

        byte[] blockDefinitions = new byte[0x400 * 8];
        blockDefinitions[0x55 * 8] = 0x55;
        var level = new RoomLevelData(width, width,
            new ushort[width * width], new byte[width * width],
            new ushort[width * width], blockDefinitions);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var plms = new RoomPlmSystem { EyeDoorVisuals = visuals };
        BindEyeDoorFixturePrograms(plms, bus);
        var guarded = new EyeDoorDrawReadGuard(bus, lists);
        AssertEqual(3, plms.LoadRoomPopulation(guarded, level, streamer,
                new SnesVram(), RoomPlmPopulationImporter.Read(guarded, 0x9000), new Bank80SystemState(), AreaId.Brinstar,
                () => new SamusState(), () => false,
                spawnEyeDoorProjectile: _ => { }),
            $"{orientation} eye-door three-component population loads");
        plms.Step(guarded, level, streamer, 0, 0, 0);
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
            ? [RoomPlmHeaders.EyeDoorEyeFacingLeft,
                RoomPlmHeaders.EyeDoorFacingLeft,
                RoomPlmHeaders.EyeDoorBottomFacingLeft]
            : [RoomPlmHeaders.EyeDoorEyeFacingRight,
                RoomPlmHeaders.EyeDoorFacingRight,
                RoomPlmHeaders.EyeDoorBottomFacingRight];
        var bank84 = new byte[0x8000];
        for (int offset = 0; offset < bank84.Length; offset++)
            bank84[offset] = rom.ReadByte(0x848000 + offset);
        var bus = new TestAddressSpace();
        bus.WriteBytes(0x848000, bank84);
        const ushort population = 0x9000;
        bus.WriteBytes(0x8f0000 | population,
        [
            unchecked((byte)headers[0]), unchecked((byte)(headers[0] >> 8)), eyeX, eyeY,
            unchecked((byte)doorBit), 0,
            unchecked((byte)headers[1]), unchecked((byte)(headers[1] >> 8)), eyeX, eyeY + 2,
            unchecked((byte)doorBit), 0,
            unchecked((byte)headers[2]), unchecked((byte)(headers[2] >> 8)), eyeX, eyeY + 4,
            unchecked((byte)doorBit), 0,
            0, 0,
        ]);
        var level = new RoomLevelData(width, width,
            new ushort[width * width], new byte[width * width],
            new ushort[width * width], new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var samus = new SamusState
        {
            XPosition = eyeX * 16 + 8,
            YPosition = eyeY * 16 + 8,
        };
        var requests = new List<EyeDoorProjectileRequest>();
        var system = new Bank80SystemState();
        var plms = new RoomPlmSystem();
        var guarded = new EyeDoorDrawReadGuard(bus, lists);
        AssertEqual(3, plms.LoadRoomPopulation(guarded, level, streamer,
                new SnesVram(), RoomPlmPopulationImporter.Read(guarded, population), system, AreaId.Brinstar,
                () => samus, () => false, spawnEyeDoorProjectile: requests.Add),
            $"{orientation} retail eye-door program loads all three components");

        int eyeBlock = eyeY * width + eyeX;
        for (int frame = 0; frame < 1024 &&
             plms.EyeDoors.Single(door => door.Component == EyeDoorComponent.Eye)
                 .PreInstruction != EyeDoorPlmRomData.MissileHitPreInstruction; frame++)
            plms.Step(guarded, level, streamer, 0, 0, 0);
        AssertEqual(EyeDoorPlmRomData.MissileHitPreInstruction,
            plms.EyeDoors.Single(door => door.Component == EyeDoorComponent.Eye)
                .PreInstruction,
            $"{orientation} retail eye-door program arms its missile pre-instruction");
        AssertTrue(plms.TryNotifyColoredDoorHit(eyeBlock,
                new SamusProjectileTypeWord(0x0200)),
            $"{orientation} retail eye door accepts a Super Missile collision");
        plms.Step(guarded, level, streamer, 0, 0, 0);
        for (int frame = 0; frame < 1024 && plms.ActiveCount != 0; frame++)
            plms.Step(guarded, level, streamer, 0, 0, 0);
        AssertTrue(system.HasOpenedDoorBit(doorBit),
            $"{orientation} retail eye-door opening persists the door bit");
        AssertEqual(0, plms.ActiveCount,
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

        var reopenedLevel = new RoomLevelData(width, width,
            new ushort[width * width], new byte[width * width],
            new ushort[width * width], new byte[0x400 * 8]);
        BackgroundTilemapStreamer reopenedStreamer =
            reopenedLevel.CreateBackgroundStreamer();
        var reopened = new RoomPlmSystem();
        AssertEqual(3, reopened.LoadRoomPopulation(guarded, reopenedLevel,
                reopenedStreamer, new SnesVram(), RoomPlmPopulationImporter.Read(guarded, population), system, AreaId.Brinstar,
                () => samus, () => false, spawnEyeDoorProjectile: requests.Add),
            $"{orientation} opened eye-door room reloads three components");
        for (int frame = 0; frame < 16 && reopened.ActiveCount != 0; frame++)
            reopened.Step(guarded, reopenedLevel, reopenedStreamer, 0, 0, 0);
        AssertEqual(0, reopened.ActiveCount,
            $"{orientation} opened eye-door reload converts to blue and removes passive parts");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"{orientation} opened eye-door reload avoids instruction and draw ROM reads");
    }

    private static void VerifyEyeDoorVisualInstallation(SuperMetroidAddressSpace rom)
    {
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "eye-door-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Eye-door test root escaped test-temp.");
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

    private sealed class EyeDoorDrawReadGuard(
        ISnesAddressSpace source,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists) : ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (address is >= 0x84d81e and <= 0x84da8b)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Eye door reread compiled instruction ${address:X6}.");
            }
            foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
            {
                int first = 0x840000 | list.Pointer;
                int length = 4 + list.Runs.Span[0].LevelWords.Length * 2;
                if (address >= first && address < first + length)
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Eye door reread compiled draw payload ${address:X6}.");
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
