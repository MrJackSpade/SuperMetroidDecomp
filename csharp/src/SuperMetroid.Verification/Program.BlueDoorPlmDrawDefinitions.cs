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
        static ushort ReadNativeWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        RoomPlmShotBlockDrawDefinitions.DrawList[] lists =
            BlueDoorPlmDrawDefinitions.All.OrderBy(list => list.Pointer).ToArray();
        RoomPlmBlueDoorVisualEntry[] entries = lists.Select(draw =>
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
        AssertEqual(16, lists.Length, "four orientations each have four blue-cap frames");
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
        {
            ReadOnlySpan<RoomPlmShotBlockDrawDefinitions.Run> runs = list.Runs.Span;
            AssertEqual(1, runs.Length, $"blue-cap ${list.Pointer:X4} has one run");
            RoomPlmShotBlockDrawDefinitions.Run run = runs[0];
            int source = 0x840000 | list.Pointer;
            AssertEqual(run.DirectionAndCount, ReadNativeWord(rom, source),
                $"blue-cap ${list.Pointer:X4} direction/count matches ROM");
            AssertEqual(4, run.LevelWords.Length,
                $"blue-cap ${list.Pointer:X4} has four physical words");
            for (int block = 0; block < 4; block++)
                AssertEqual(run.LevelWords.Span[block], ReadNativeWord(rom, source + 2 + block * 2),
                    $"blue-cap ${list.Pointer:X4} block {block} matches ROM");
            AssertEqual((ushort)0, ReadNativeWord(rom, source + 10),
                $"blue-cap ${list.Pointer:X4} ends at its signed-offset terminator");
        }

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
                $"{orientation} opening avoids all blue-cap draw-list ROM bytes");
            if (orientation == ColoredDoorOrientation.Left)
            {
                AssertEqual((ushort)0x0053,
                    plms.TilemapUpdates[0].TopRow[0],
                    "edited blue-door tile reaches immediate redraw");
                AssertEqual((ushort)0x0053,
                    level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(origin, 0).TopRow[0],
                    "edited blue-door tile survives later camera streaming");
            }
        }

        AssertThrows<InvalidDataException>(
            () => new RoomPlmBlueDoorVisualCatalog(entries.Skip(1)),
            "blue-door catalog rejects missing frames");
        editedFrame.Blocks[0] = 0xf053;
        AssertThrows<InvalidDataException>(
            () => new RoomPlmBlueDoorVisualCatalog(entries),
            "blue-door catalog rejects collision bits in visual words");
        editedFrame.Blocks[0] = originalVisual;
        VerifyBlueDoorVisualInstallation(rom, leftFirstDraw);

        Console.WriteLine("  Blue-door PLM draws: sixteen exact cartridge lists; four native opening paths reject ROM payload reads; editable stock/override presentation preserves collision.");
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
            ushort stock = installation.LoadRoomPlmBlueDoorVisuals().GetWord(firstDraw, 0);

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
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists) : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
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
