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
        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        RoomPlmShotBlockDrawDefinitions.DrawList[] lists =
            EyeDoorPlmDrawDefinitions.All.OrderBy(list => list.Pointer).ToArray();
        RoomPlmEyeDoorVisualEntry[] entries = lists.Select(draw =>
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
        AssertEqual(stockWord, RoomPlmEyeDoorVisualCatalog.Stock().GetWord(0x9c5b, 0),
            "stock eye-door catalog retains native frame");
        rightEye.Blocks[0] = stockWord;
        AssertEqual(23, lists.Length,
            "mirrored eye, middle and bottom components select 23 distinct draw lists");
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
        {
            AssertEqual(1, list.Runs.Length,
                $"eye-door draw ${list.Pointer:X4} has one run");
            RoomPlmShotBlockDrawDefinitions.Run run = list.Runs.Span[0];
            int source = 0x840000 | list.Pointer;
            AssertEqual(run.DirectionAndCount, ReadWord(rom, source),
                $"eye-door draw ${list.Pointer:X4} direction/count matches ROM");
            for (int block = 0; block < run.LevelWords.Length; block++)
                AssertEqual(run.LevelWords.Span[block],
                    ReadWord(rom, source + 2 + block * 2),
                    $"eye-door draw ${list.Pointer:X4} block {block} matches ROM");
            AssertEqual((ushort)0,
                ReadWord(rom, source + 2 + run.LevelWords.Length * 2),
                $"eye-door draw ${list.Pointer:X4} terminates after its physical words");
        }

        VerifyEyeDoorNativeDrawPath(EyeDoorOrientation.Left, lists, null);
        VerifyEyeDoorNativeDrawPath(EyeDoorOrientation.Right, lists, edited);
        AssertThrows<InvalidDataException>(
            () => new RoomPlmEyeDoorVisualCatalog(entries.Skip(1)),
            "eye-door catalog rejects missing frames");
        rightEye.Blocks[0] = 0xf055;
        AssertThrows<InvalidDataException>(
            () => new RoomPlmEyeDoorVisualCatalog(entries),
            "eye-door catalog rejects collision bits in visual words");
        rightEye.Blocks[0] = stockWord;
        VerifyEyeDoorVisualInstallation(rom);
        Console.WriteLine(
            "  Eye-door PLM draws: 23 native lists, guarded mirrored assemblies, and editable stock/override appearance preserve collision.");
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
        var guarded = new EyeDoorDrawReadGuard(bus, lists);
        AssertEqual(3, plms.LoadRoomPopulation(guarded, level, streamer,
                new SnesVram(), 0x9000, new Bank80SystemState(), AreaId.Brinstar,
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
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists) : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
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
