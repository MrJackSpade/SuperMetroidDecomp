using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyNoobTubePlmDrawDefinitions(SuperMetroidAddressSpace rom)
    {
        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        RoomPlmShotBlockDrawDefinitions.DrawList[] lists =
            NoobTubePlmDrawDefinitions.All.OrderBy(list => list.Pointer).ToArray();
        RoomPlmNoobTubeVisualEntry[] entries = lists.Select(draw =>
            new RoomPlmNoobTubeVisualEntry(
                NoobTubePlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span.ToArray().SelectMany(run =>
                    run.LevelWords.Span.ToArray().Select(word =>
                        new RoomLevelWord(word).VisualWord)).ToArray())).ToArray();
        RoomPlmNoobTubeVisualEntry cleared = entries.Single(entry =>
            entry.Id == "cleared");
        ushort originalWord = cleared.Blocks[24];
        cleared.Blocks[24] = 0x0059;
        var edited = new RoomPlmNoobTubeVisualCatalog(entries);
        cleared.Blocks[24] = 0x005a;
        AssertEqual((ushort)0x0059, edited.GetWord(0x98e3, 2, 0),
            "n00b-tube catalog copies author data");
        AssertEqual(originalWord,
            RoomPlmNoobTubeVisualCatalog.Stock().GetWord(0x98e3, 2, 0),
            "stock n00b-tube catalog retains the native row tile");
        cleared.Blocks[24] = originalWord;
        AssertEqual(7, lists.Length,
            "n00b-tube program selects seven distinct physical draw lists");
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
        {
            int cursor = 0x840000 | list.Pointer;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                AssertEqual(run.DirectionAndCount, ReadWord(rom, cursor),
                    $"n00b-tube draw ${list.Pointer:X4} direction/count at ${cursor:X6}");
                for (int block = 0; block < run.LevelWords.Length; block++)
                    AssertEqual(run.LevelWords.Span[block],
                        ReadWord(rom, cursor + 2 + block * 2),
                        $"n00b-tube draw ${list.Pointer:X4} physical block {block}");
                ushort offset = (ushort)((byte)run.NextX | ((byte)run.NextY << 8));
                AssertEqual(offset,
                    ReadWord(rom, cursor + 2 + run.LevelWords.Length * 2),
                    $"n00b-tube draw ${list.Pointer:X4} signed next-run offset");
                cursor += 4 + run.LevelWords.Length * 2;
            }
        }

        var bank84 = new byte[0x8000];
        for (int index = 0; index < bank84.Length; index++)
            bank84[index] = rom.ReadByte(0x848000 + index);
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
            VerifyNoobTubeNativeDrawPath(bank84, lists, list,
                list.Pointer == 0x98e3 ? edited : null);
        AssertThrows<InvalidDataException>(
            () => new RoomPlmNoobTubeVisualCatalog(entries.Skip(1)),
            "n00b-tube catalog rejects missing frames");
        cleared.Blocks[24] = 0xf059;
        AssertThrows<InvalidDataException>(
            () => new RoomPlmNoobTubeVisualCatalog(entries),
            "n00b-tube catalog rejects collision bits in visual words");
        cleared.Blocks[24] = originalWord;
        VerifyNoobTubeVisualInstallation(rom);
        Console.WriteLine(
            "  N00b-tube PLM: seven guarded native layouts and editable stock/override appearance preserve physical blocks.");
    }

    private static void VerifyNoobTubeNativeDrawPath(
        byte[] bank84,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists,
        RoomPlmShotBlockDrawDefinitions.DrawList selected,
        RoomPlmNoobTubeVisualCatalog? visuals)
    {
        const int width = 16;
        const int height = 16;
        const int originX = 2;
        const int originY = 2;
        var bus = new TestAddressSpace();
        bus.WriteBytes(0x848000, bank84);
        bus.WriteBytes(0x8f9400,
        [
            unchecked((byte)RoomPlmHeaders.NoobTube),
            unchecked((byte)(RoomPlmHeaders.NoobTube >> 8)),
            originX, originY, 0, 0, 0, 0,
        ]);
        // Preserve the cartridge control stream, varying only its first draw
        // operand to exercise every native layout through the actual PLM caller.
        WriteWord(bus, 0x84d4e4, selected.Pointer);
        var guarded = new NoobTubeDrawReadGuard(bus, lists);
        byte[] blockDefinitions = new byte[0x400 * 8];
        blockDefinitions[0x59 * 8] = 0x59;
        var level = new RoomLevelData(width, height,
            new ushort[width * height], new byte[width * height],
            new ushort[width * height], blockDefinitions);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var plms = new RoomPlmSystem { NoobTubeVisuals = visuals };
        AssertEqual(1, plms.LoadRoomPopulation(guarded, level, streamer,
                new SnesVram(), 0x9400, new Bank80SystemState(), AreaId.Maridia,
                () => new SamusState(), () => false,
                hasEvent: _ => false,
                setEvent: _ => { }),
            $"n00b tube loads for draw ${selected.Pointer:X4}");
        plms.Step(guarded, level, streamer, 0, 0, 0);

        int entryX = originX;
        int entryY = originY;
        var expected = new Dictionary<int, ushort>();
        foreach (RoomPlmShotBlockDrawDefinitions.Run run in selected.Runs.Span)
        {
            bool vertical = (run.DirectionAndCount & 0x8000) != 0;
            for (int block = 0; block < run.LevelWords.Length; block++)
            {
                int x = entryX + (vertical ? 0 : block);
                int y = entryY + (vertical ? block : 0);
                expected.Add(y * width + x, run.LevelWords.Span[block]);
            }
            entryX = originX + run.NextX;
            entryY = originY + run.NextY;
        }
        foreach ((int index, ushort word) in expected)
            AssertEqual(word, level.GetCollisionBlockByIndex(index).LevelWord,
                $"n00b-tube draw ${selected.Pointer:X4} writes native physical block {index}");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"n00b-tube draw ${selected.Pointer:X4} avoids source payload reads");
        if (visuals is not null)
        {
            const int editedBlock = 4 * width + 2;
            AssertTrue(plms.TilemapUpdates.Any(update => update.TopRow[0] == 0x0059),
                "edited twelve-block tube row reaches the immediate PLM update");
            AssertEqual((ushort)0x0059,
                level.CreateBackgroundStreamer()
                    .BuildPlmLevelBlockUpdate(editedBlock, 0).TopRow[0],
                "edited tube tile survives later camera streaming");
            AssertEqual((ushort)0x0323,
                level.GetCollisionBlockByIndex(editedBlock).LevelWord,
                "edited tube appearance does not change the physical block");
        }
    }

    private static void VerifyNoobTubeVisualInstallation(SuperMetroidAddressSpace rom)
    {
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "noob-tube-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Tube visual test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmNoobTubeVisualFiles.Extract(rom,
                installation.RoomPlmNoobTubeVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmNoobTubeVisualFiles.ValidateStock(
                installation.RoomPlmNoobTubeVisualDirectory);
            string stockPath = Path.Combine(
                installation.RoomPlmNoobTubeVisualDirectory,
                RoomPlmNoobTubeVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted tube JSON is empty.");
            JsonNode frame = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() == "cleared")!;
            frame["blocks"]![24] = 0x0059;
            Directory.CreateDirectory(
                installation.RoomPlmNoobTubeVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmNoobTubeVisualOverrideDirectory,
                RoomPlmNoobTubeVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0059,
                installation.LoadRoomPlmNoobTubeVisuals().GetWord(0x98e3, 2, 0),
                "installed tube override changes an internal twelve-block row tile");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmNoobTubeVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0059,
                RoomPlmNoobTubeVisualFiles.Load(refreshed,
                    installation.RoomPlmNoobTubeVisualOverrideDirectory)
                    .GetWord(0x98e3, 2, 0),
                "tube override survives stock replacement");

            frame["blocks"]![24] = 0xf059;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmNoobTubeVisuals(),
                "invalid tube override fails loudly");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmNoobTubeVisuals(),
                "tampered tube stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    private sealed class NoobTubeDrawReadGuard(
        ISnesAddressSpace source,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists) : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
            {
                int first = 0x840000 | list.Pointer;
                int length = list.Runs.Span.ToArray().Sum(run =>
                    4 + run.LevelWords.Length * 2);
                if (address >= first && address < first + length)
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"N00b tube reread draw payload ${address:X6}.");
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
