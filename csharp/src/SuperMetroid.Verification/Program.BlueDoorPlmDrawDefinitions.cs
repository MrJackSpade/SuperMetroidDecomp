using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyBlueDoorPlmDrawDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyBlueDoorProgramDefinitions), () => VerifyBlueDoorProgramDefinitions(rom));
        static ushort ReadNativeWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        RoomPlmShotBlockDrawDefinitions.DrawList[] lists =
            BlueDoorPlmDrawDefinitionsTooling.All.OrderBy(list => list.Pointer).ToArray();
        RoomPlmBlueDoorVisualEntry[] entries = BlueDoorPlmDrawDefinitions.Editable.Select(draw =>
            new RoomPlmBlueDoorVisualEntry(BlueDoorPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span[0].LevelWords.Span.ToArray()
                    .Select(word => new RoomLevelWord(word).VisualWord).ToArray())).ToArray();
        RoomPlmBlueDoorVisualCatalog stock = RoomPlmBlueDoorVisualCatalog.Stock();
        RoomPlmBlueDoorVisualEntry editedFrame = entries.Single(entry =>
            entry.Id == "left-frame-1");
        ushort originalVisual = editedFrame.Blocks[0];
        editedFrame.Blocks[0] = 0x0053;
        var edited = new RoomPlmBlueDoorVisualCatalog(entries);
        editedFrame.Blocks[0] = 0x0054;
        ushort leftFirstDraw = ReadNativeWord(rom,
            0x840000 | (RoomPlmInstructionLists.BlueDoorFacingLeftOpening + 5));
        AssertEqual((ushort)0x0053, edited.GetWord(leftFirstDraw, 0),
            "blue-door visual catalog copies author data");
        AssertEqual(originalVisual, stock.GetWord(leftFirstDraw, 0),
            "stock blue-door visual retains native tile choice");
        Suite(nameof(VerifyBlueCapGeometry), () => VerifyBlueCapGeometry(rom));
        Suite(nameof(VerifyBlueCapCollision), () => VerifyBlueCapCollision(rom));
        Suite(nameof(VerifyBlueCapVisuals), () => VerifyBlueCapVisuals(rom));

        foreach (ColoredDoorOrientation orientation in Enum.GetValues<ColoredDoorOrientation>())
        {
            ushort openingList = orientation switch
            {
                ColoredDoorOrientation.Left => RoomPlmInstructionLists.BlueDoorFacingLeftOpening,
                ColoredDoorOrientation.Right => RoomPlmInstructionLists.BlueDoorFacingRightOpening,
                ColoredDoorOrientation.Up => RoomPlmInstructionLists.BlueDoorFacingUpOpening,
                ColoredDoorOrientation.Down => RoomPlmInstructionLists.BlueDoorFacingDownOpening,
                _ => throw new InvalidDataException("Unknown blue-door orientation."),
            };
            ushort firstDraw = ReadNativeWord(rom, 0x840000 | (openingList + 5));
            AssertTrue(BlueDoorPlmDrawDefinitions.TryGet(firstDraw, out var selected),
                $"{orientation} opening program selects a compiled draw");

            const int width = 16;
            const int origin = 4 + 4 * width;
            byte[] blockDefinitions = new byte[0x400 * 8];
            blockDefinitions[originalVisual * 8] = 0x0d;
            blockDefinitions[0x53 * 8] = 0x53;
            var level = new RoomLevelData(width, width,
                new ushort[width * width], new byte[width * width],
                new ushort[width * width], blockDefinitions);
            var plms = new RoomPlmSystem
            {
                BlueDoorVisuals = orientation == ColoredDoorOrientation.Left ? edited : stock,
            };
            RoomBlockBehavior behavior = new(unchecked((byte)(
                RoomBlockBehaviorValues.BlueDoorFacingLeft.Value + (byte)orientation)));
            AssertTrue(plms.TrySpawnBlueDoorOpening(level, origin, behavior,
                new SamusProjectileTypeWord(0)),
                $"{orientation} blue door allocates its native opening actor");
            var guarded = new BlueDoorDrawReadGuard(rom, lists);
            BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
            plms.Step(guarded, level, streamer, 0, 0, 0);
            int stride = orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right
                ? width : 1;
            for (int block = 0; block < 4; block++)
                AssertEqual(selected.Runs.Span[0].LevelWords.Span[block],
                    level.GetCollisionBlockByIndex(origin + block * stride).LevelWord,
                    $"{orientation} native draw writes physical block {block} without ROM payload reads");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{orientation} opening avoids compiled program and draw ROM bytes");
            if (orientation == ColoredDoorOrientation.Left)
            {
                AssertEqual((ushort)0x0053,
                    plms.TilemapUpdates[0].TopRow[0],
                    "edited blue-door tile reaches immediate redraw");
                AssertEqual((ushort)0x0053,
                    level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(origin, 0).TopRow[0],
                    "edited blue-door tile survives later camera streaming");
            }

            // Continue through every opening draw, the long air-cap hold and Delete.
            for (int frame = 0; frame < 128 && plms.ActiveCount != 0; frame++)
                plms.Step(guarded, level, streamer, 0, 0, 0);
            AssertEqual(0, plms.ActiveCount,
                $"{orientation} blue-door opening reaches its native Delete");

            ushort closingList = orientation switch
            {
                ColoredDoorOrientation.Left => BlueDoorPlmProgramDefinitions.ClosingLeft,
                ColoredDoorOrientation.Right => BlueDoorPlmProgramDefinitions.ClosingRight,
                ColoredDoorOrientation.Up => BlueDoorPlmProgramDefinitions.ClosingUp,
                ColoredDoorOrientation.Down => BlueDoorPlmProgramDefinitions.ClosingDown,
                _ => throw new InvalidDataException("Unknown blue-door orientation."),
            };
            ushort closedList = orientation switch
            {
                ColoredDoorOrientation.Left => BlueDoorPlmProgramDefinitions.ClosedLeft,
                ColoredDoorOrientation.Right => BlueDoorPlmProgramDefinitions.ClosedRight,
                ColoredDoorOrientation.Up => BlueDoorPlmProgramDefinitions.ClosedUp,
                ColoredDoorOrientation.Down => BlueDoorPlmProgramDefinitions.ClosedDown,
                _ => throw new InvalidDataException("Unknown blue-door orientation."),
            };
            foreach (ushort list in new[] { closingList, closedList })
            {
                AssertTrue(plms.TrySpawnBlueDoorOpening(level, origin, behavior,
                    new SamusProjectileTypeWord(0)),
                    $"{orientation} allocates actor for list ${list:X4}");
                plms.SetSoleInstructionPointerForVerification(list);
                for (int frame = 0; frame < 20 && plms.ActiveCount != 0; frame++)
                    plms.Step(guarded, level, streamer, 0, 0, 0);
                AssertEqual(0, plms.ActiveCount,
                    $"{orientation} list ${list:X4} reaches its native Delete");
                AssertEqual((int)behavior.Value,
                    level.GetCollisionBlockByIndex(origin).Behavior,
                    $"{orientation} list ${list:X4} restores the blue-door BTS");
            }
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{orientation} complete blue-door lists avoid all compiled ROM sources");
        }

        editedFrame.Blocks[0] = originalVisual;
        Suite(nameof(VerifyBlueDoorVisualInstallation), () => VerifyBlueDoorVisualInstallation(rom, leftFirstDraw));

        Console.WriteLine("  Blue-door PLMs: 196 compiled program bytes, all opening/closing/closed lists, twenty physical draws and sixteen compatible visual identities pass with source reads forbidden.");
    }

    private static void VerifyBlueDoorProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyBlueProgramControls), () => VerifyBlueProgramControls(rom));
        Suite(nameof(VerifyBlueProgramDraws), () => VerifyBlueProgramDraws(rom));
        Suite(nameof(VerifyBlueProgramSounds), () => VerifyBlueProgramSounds(rom));
        Suite(nameof(VerifyBlueProgramBts), () => VerifyBlueProgramBts(rom));
    }

    private static void VerifyBlueDoorVisualInstallation(
        SuperMetroidAddressSpace rom, ushort firstDraw)
    {
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "blue-door-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Blue-door test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmBlueDoorVisualFiles.Extract(rom,
                installation.RoomPlmBlueDoorVisualDirectory, SupportedCartridge.Sha256);
            RoomPlmBlueDoorVisualFiles.ValidateStock(
                installation.RoomPlmBlueDoorVisualDirectory);
            var installed = installation.LoadRoomPlmBlueDoorVisuals();
            VerifyBlueDoorStockMapping(rom, installed);
            ushort stock = installed.GetWord(firstDraw, 0);

            string stockPath = Path.Combine(installation.RoomPlmBlueDoorVisualDirectory,
                RoomPlmBlueDoorVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted blue-door JSON is empty.");
            JsonNode frame = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() == "left-frame-1")!;
            frame["blocks"]![0] = 0x0053;
            Directory.CreateDirectory(installation.RoomPlmBlueDoorVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmBlueDoorVisualOverrideDirectory,
                RoomPlmBlueDoorVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0053,
                installation.LoadRoomPlmBlueDoorVisuals().GetWord(firstDraw, 0),
                "installed blue-door override changes the selected frame");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmBlueDoorVisualFiles.Extract(rom, refreshed, SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmBlueDoorVisualFiles.Load(refreshed,
                    installation.RoomPlmBlueDoorVisualOverrideDirectory).GetWord(firstDraw, 0),
                "blue-door override survives stock replacement");

            frame["blocks"]![0] = 0xf053;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmBlueDoorVisuals(),
                "invalid blue-door override fails loudly");
            frame["blocks"]![0] = stock;
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmBlueDoorVisuals(),
                "tampered blue-door stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    private sealed class BlueDoorDrawReadGuard(
        ISnesAddressSpace source,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists) : ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (address is >= 0x84c489 and <= 0x84c54c)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production blue-door instruction reread bank-$84 byte ${address:X6}.");
            }
            foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
            {
                if (address >= (0x840000 | list.Pointer) &&
                    address < (0x840000 | list.Pointer) + BlueDoorPlmDrawDefinitions.DrawListBytes)
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Production blue-door draw reread bank-$84 payload ${address:X6}.");
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
