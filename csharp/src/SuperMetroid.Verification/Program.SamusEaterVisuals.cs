using System.Reflection;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifySamusEaterVisuals()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)),
            "Samus Eater native oracle revision");
        Suite(nameof(VerifySamusEaterDrawGeometry), () => VerifySamusEaterDrawGeometry(rom));
        Suite(nameof(VerifySamusEaterDrawWords), () => VerifySamusEaterDrawWords(rom));
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "samus-eater-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Samus Eater test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmSamusEaterVisualFiles.Extract(rom,
                installation.RoomPlmSamusEaterVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmSamusEaterVisualFiles.ValidateStock(
                installation.RoomPlmSamusEaterVisualDirectory);
            VerifySamusEaterStockVisualMapping(rom, installation.LoadRoomPlmSamusEaterVisuals());

            string stockPath = Path.Combine(
                installation.RoomPlmSamusEaterVisualDirectory,
                RoomPlmSamusEaterVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted Samus Eater JSON is empty.");
            JsonArray entries = document["entries"]!.AsArray();
            JsonNode floor = entries.Single(entry =>
                entry!["id"]!.GetValue<string>() == "floor-chew-2")!;
            JsonNode ceiling = entries.Single(entry =>
                entry!["id"]!.GetValue<string>() == "ceiling-chew-2")!;
            floor["blocks"]![0] = 0x0058;
            ceiling["blocks"]![0] = 0x0059;
            Directory.CreateDirectory(
                installation.RoomPlmSamusEaterVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmSamusEaterVisualOverrideDirectory,
                RoomPlmSamusEaterVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            RoomPlmSamusEaterVisualCatalog edited =
                installation.LoadRoomPlmSamusEaterVisuals();
            VerifySamusEaterLiveDraw(rom, edited,
                SamusEaterPlmDrawDefinitions.FloorChew2, 0x05a5, 0x0058);
            VerifySamusEaterLiveDraw(rom, edited,
                SamusEaterPlmDrawDefinitions.CeilingChew2, 0x0da5, 0x0059);

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmSamusEaterVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0058,
                RoomPlmSamusEaterVisualFiles.Load(refreshed,
                    installation.RoomPlmSamusEaterVisualOverrideDirectory)
                    .GetWord(SamusEaterPlmDrawDefinitions.FloorChew2, 0, 0),
                "Samus Eater override survives stock refresh");
            floor["blocks"]![0] = 0xf058;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmSamusEaterVisuals(),
                "Samus Eater override cannot change collision nibble");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmSamusEaterVisualFiles.ValidateStock(
                    installation.RoomPlmSamusEaterVisualDirectory),
                "tampered Samus Eater stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
        Console.WriteLine("Samus Eater visuals: eight exact native draw lists, editable live floor/ceiling art, physical isolation, stock integrity and repair pass.");
    }

    private static void VerifySamusEaterStockVisualMapping(SuperMetroidAddressSpace rom,
        RoomPlmSamusEaterVisualCatalog installed)
    {
        (ushort Pointer, string Id)[] frames = [(0x9e0d,"floor-idle"), (0x9e45,"floor-chew-1"),
            (0x9e61,"floor-chew-2"), (0x9e7d,"floor-chew-3"), (0x9e99,"ceiling-idle"),
            (0x9ed1,"ceiling-chew-1"), (0x9eed,"ceiling-chew-2"), (0x9f09,"ceiling-chew-3")];
        var native = new Dictionary<ushort, ushort[]>();
        var counts = new Dictionary<ushort, int[]>();
        foreach (var frame in frames)
        {
            var words = new List<ushort>();
            var runs = new List<int>();
            int cursor = 0x840000 | frame.Pointer;
            while (true)
            {
                int count = ReadSamusEaterPlmWord(rom, cursor) & 0x7fff;
                runs.Add(count);
                for (int block = 0; block < count; block++)
                    words.Add((ushort)(ReadSamusEaterPlmWord(rom, cursor + 2 + block * 2) & 0xfff));
                cursor += 2 + count * 2;
                ushort offset = ReadSamusEaterPlmWord(rom, cursor);
                cursor += 2;
                if (offset == 0) break;
            }
            native.Add(frame.Pointer, words.ToArray());
            counts.Add(frame.Pointer, runs.ToArray());
        }
        var entries = frames.Select(frame => new RoomPlmSamusEaterVisualEntry(frame.Id, native[frame.Pointer].ToArray())).ToArray();
        var stock = RoomPlmSamusEaterVisualCatalog.Stock();
        var imported = new RoomPlmSamusEaterVisualCatalog(entries.Reverse());
        string Hash(Dictionary<ushort, ushort[]> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(
            nameof(RoomPlmSamusEaterVisualCatalog), words);
        foreach (var catalog in new[] {stock, imported, installed})
        {
            AssertEqual(Hash(native), catalog.ContentIdentity, "plant native flattened stock hash");
            foreach (var frame in frames)
            {
                int index = 0;
                for (int run = 0; run < counts[frame.Pointer].Length; run++)
                {
                    int count = counts[frame.Pointer][run];
                    for (int block = 0; block < count; block++)
                        AssertEqual(native[frame.Pointer][index++], catalog.GetWord(frame.Pointer, run, block), "plant native stock appearance");
                    foreach (int bad in new[] {int.MinValue, -1, count, int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,run,bad), "plant stock block bounds");
                }
                foreach (int bad in new[] {int.MinValue, -1, counts[frame.Pointer].Length, int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,bad,0), "plant stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetWord((ushort)pointer,0,0), "plant stock rejects unknown pointer");
        entries[0].Blocks[7] = 0x0c58;
        entries[6].Blocks[0] = 0x0059;
        var mixed = new RoomPlmSamusEaterVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer, frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "plant mixed custom and stock hash");
        entries[0].Blocks[7] = 0x005a;
        entries[1].Blocks[0] = 0x005b;
        foreach (var frame in frames)
        {
            int index = 0;
            for (int run = 0; run < counts[frame.Pointer].Length; run++)
            for (int block = 0; block < counts[frame.Pointer][run]; block++)
                AssertEqual(expected[frame.Pointer][index++], mixed.GetWord(frame.Pointer,run,block), "plant custom cloning and calculated stock isolation");
        }
        AssertThrows<InvalidDataException>(() => new RoomPlmSamusEaterVisualCatalog(entries[..7]), "plant missing frame rejected");
        AssertThrows<InvalidDataException>(() => new RoomPlmSamusEaterVisualCatalog(entries.Append(entries[0])), "plant duplicate frame rejected");
        var badEntries = entries.ToArray();
        badEntries[0] = new("FLOOR-IDLE", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => new RoomPlmSamusEaterVisualCatalog(badEntries), "plant IDs remain ordinal");
        badEntries[0] = new(entries[0].Id, new ushort[7]);
        AssertThrows<InvalidDataException>(() => new RoomPlmSamusEaterVisualCatalog(badEntries), "plant wrong shape rejected");
        entries[0].Blocks[0] = 0xf058;
        AssertThrows<InvalidDataException>(() => new RoomPlmSamusEaterVisualCatalog(entries), "plant collision bits rejected");
    }

    private static void VerifySamusEaterDrawGeometry(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySamusEaterDrawField), () => VerifySamusEaterDrawField(rom, false));

    private static void VerifySamusEaterDrawWords(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySamusEaterDrawField), () => VerifySamusEaterDrawField(rom, true));

    private static void VerifySamusEaterDrawField(SuperMetroidAddressSpace rom, bool words)
    {
        ushort[] pointers = [0x9e0d, 0x9e45, 0x9e61, 0x9e7d, 0x9e99, 0x9ed1, 0x9eed, 0x9f09];
        var exported = SamusEaterPlmDrawDefinitions.All.ToArray();
        AssertEqual(pointers.Length, exported.Length, "plant draw export coverage");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort pointer = (ushort)raw;
            int frame = Array.IndexOf(pointers, pointer);
            AssertEqual(frame >= 0, SamusEaterPlmDrawDefinitions.TryDescribe(pointer, out var draw),
                "plant draw pointer ownership includes only supported records");
            AssertEqual(frame >= 0, SamusEaterPlmDrawDefinitions.TryGet(pointer, out var dto),
                "plant draw DTO pointer ownership");
            if (frame < 0)
            {
                AssertEqual(default(SamusEaterPlmDrawDefinitions.Draw), draw, "missing plant descriptor");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "missing plant DTO");
                continue;
            }
            AssertEqual(pointer, exported[frame].Pointer, "plant export native order");
            AssertEqual(3, dto.Runs.Length, "plant run count");
            int cursor = 0x840000 | pointer;
            for (int run = 0; run < 3; run++)
            {
                ushort count = ReadSamusEaterPlmWord(rom, cursor);
                var exportedRun = exported[frame].Runs.Span[run];
                var dtoRun = dto.Runs.Span[run];
                if (words)
                {
                    for (int block = 0; block < count; block++)
                    {
                        ushort native = ReadSamusEaterPlmWord(rom, cursor + 2 + block * 2);
                        AssertEqual(native, draw.WordAt(run, block), "calculated plant physical word");
                        AssertEqual(native, dtoRun.LevelWords.Span[block], "direct plant DTO word");
                        AssertEqual(native, exportedRun.LevelWords.Span[block], "exported plant word");
                    }
                    foreach (int invalid in new[] { int.MinValue, -1, (int)count, 256, int.MaxValue })
                        AssertThrows<IndexOutOfRangeException>(() => draw.WordAt(run, invalid), "plant block bounds");
                }
                else
                {
                    AssertEqual((int)count, SamusEaterPlmDrawDefinitions.Draw.Count(run), "plant native horizontal count");
                    AssertEqual(count, dtoRun.DirectionAndCount, "plant DTO count");
                    AssertEqual(count, exportedRun.DirectionAndCount, "plant export count");
                    ushort offset = ReadSamusEaterPlmWord(rom, cursor + 2 + count * 2);
                    sbyte x = unchecked((sbyte)offset), y = unchecked((sbyte)(offset >> 8));
                    AssertEqual(x, SamusEaterPlmDrawDefinitions.Draw.NextX(run), "plant signed horizontal offset");
                    AssertEqual(y, draw.NextY(run), "plant signed vertical offset");
                    AssertEqual(x, dtoRun.NextX, "plant DTO horizontal offset");
                    AssertEqual(y, dtoRun.NextY, "plant DTO vertical offset");
                    AssertEqual(x, exportedRun.NextX, "plant export horizontal offset");
                    AssertEqual(y, exportedRun.NextY, "plant export vertical offset");
                }
                cursor += 4 + count * 2;
            }
            foreach (int invalid in new[] { int.MinValue, -1, 3, 256, int.MaxValue })
            {
                AssertThrows<IndexOutOfRangeException>(() => SamusEaterPlmDrawDefinitions.Draw.Count(invalid), "plant run count bounds");
                AssertThrows<IndexOutOfRangeException>(() => SamusEaterPlmDrawDefinitions.Draw.NextX(invalid), "plant offset X bounds");
                AssertThrows<IndexOutOfRangeException>(() => draw.NextY(invalid), "plant offset Y bounds");
                AssertThrows<IndexOutOfRangeException>(() => draw.WordAt(invalid, 0), "plant word run bounds");
            }
        }
    }

    private static void VerifySamusEaterLiveDraw(
        SuperMetroidAddressSpace rom, RoomPlmSamusEaterVisualCatalog visuals, ushort pointer,
        ushort physicalWord, ushort visualWord)
    {
        const int width = 16;
        const int height = 16;
        var level = CreateRoom(width, height, new ushort[width * height],
            new byte[width * height], blockDefinitions: new byte[0x400 * 8]);
        level.SetBlockDefinitionWord(visualWord * 4, visualWord);
        var plms = new RoomPlmSystem { SamusEaterVisuals = visuals };
        var guard = new SamusEaterVisualSourceGuard();
        var streamer = level.CreateBackgroundStreamer();
        MethodInfo draw = typeof(RoomPlmSystem)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(method => method.Name == "DrawPlmInstruction" &&
                method.GetParameters().Length == 9);
        int origin = 8 * width + 8;
        draw.Invoke(plms, [guard, level, streamer, (ushort)0, origin, pointer,
            (ushort)0, (ushort)0, (ushort)0]);
        AssertEqual(physicalWord,
            level.GetCollisionBlockByIndex(origin).LevelWord,
            "Samus Eater edited draw retains native collision word");
        // The redraw identifies its block by the block's DrawPLM BG1 ring destination.
        ushort originDestination = level.CreateBackgroundStreamer()
            .BuildPlmLevelBlockUpdate(origin, 0).TopRowDestination;
        AssertTrue(plms.TilemapUpdates.Any(update =>
                update.TopRowDestination == originDestination && update.TopRow[0] == visualWord),
            "Samus Eater edited block reaches the real redraw path");
        int nativeCursor = 0x840000 | pointer;
        int expectedX = 8, expectedY = 8;
        for (int run = 0; run < 3; run++)
        {
            int count = ReadSamusEaterPlmWord(rom, nativeCursor);
            for (int block = 0; block < count; block++)
                AssertEqual(ReadSamusEaterPlmWord(rom, nativeCursor + 2 + block * 2),
                    level.GetCollisionBlockByIndex(expectedY * width + expectedX + block).LevelWord,
                    "plant live placement follows native signed run offsets");
            ushort offset = ReadSamusEaterPlmWord(rom, nativeCursor + 2 + count * 2);
            expectedX = 8 + unchecked((sbyte)offset);
            expectedY = 8 + unchecked((sbyte)(offset >> 8));
            nativeCursor += 4 + count * 2;
        }
        AssertEqual(0, guard.Reads,
            "compiled Samus Eater draw never reads ROM at runtime");
    }

    private sealed class SamusEaterVisualSourceGuard : ISnesAddressSpace, IImportCartridgeSource
    {
        public int Reads { get; private set; }
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            Reads++;
            throw new InvalidOperationException(
                $"Samus Eater drew from native ROM ${address:X6}.");
        }

        public void WriteByte(int address, byte value) =>
            throw new InvalidOperationException("Samus Eater draw wrote to CPU bus.");
    }
}
