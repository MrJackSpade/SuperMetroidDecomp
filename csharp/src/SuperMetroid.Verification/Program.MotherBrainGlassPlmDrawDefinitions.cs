using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyMotherBrainGlassPlmDrawDefinitions(
        SuperMetroidAddressSpace rom)
    {
        VerifyMotherBrainGlassPlmProgram(rom);
        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        RoomPlmShotBlockDrawDefinitions.DrawList[] lists =
            MotherBrainGlassPlmDrawDefinitions.All.OrderBy(list => list.Pointer).ToArray();
        RoomPlmMotherBrainGlassVisualEntry[] entries = lists.Select(draw =>
            new RoomPlmMotherBrainGlassVisualEntry(
                MotherBrainGlassPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span.ToArray().SelectMany(run =>
                    run.LevelWords.Span.ToArray().Select(word =>
                        new RoomLevelWord(word).VisualWord)).ToArray())).ToArray();
        RoomPlmMotherBrainGlassVisualEntry shatter = entries.Single(entry =>
            entry.Id == "shatter-1");
        ushort originalWord = shatter.Blocks[6];
        shatter.Blocks[6] = 0x0057;
        var edited = new RoomPlmMotherBrainGlassVisualCatalog(entries);
        shatter.Blocks[6] = 0x0058;
        AssertEqual((ushort)0x0057, edited.GetWord(0x978f, 1, 2),
            "glass visual catalog copies author data");
        AssertEqual(originalWord,
            RoomPlmMotherBrainGlassVisualCatalog.Stock().GetWord(0x978f, 1, 2),
            "stock glass visual catalog retains the native shatter tile");
        shatter.Blocks[6] = originalWord;
        AssertEqual(11, lists.Length,
            "Mother Brain glass program selects eleven distinct physical draw lists");
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
        {
            int cursor = 0x840000 | list.Pointer;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                AssertEqual(run.DirectionAndCount, ReadWord(rom, cursor),
                    $"glass draw ${list.Pointer:X4} direction/count at ${cursor:X6}");
                for (int block = 0; block < run.LevelWords.Length; block++)
                    AssertEqual(run.LevelWords.Span[block],
                        ReadWord(rom, cursor + 2 + block * 2),
                        $"glass draw ${list.Pointer:X4} physical block {block}");
                ushort offset = (ushort)((byte)run.NextX | ((byte)run.NextY << 8));
                AssertEqual(offset,
                    ReadWord(rom, cursor + 2 + run.LevelWords.Length * 2),
                    $"glass draw ${list.Pointer:X4} signed next-run offset");
                cursor += 4 + run.LevelWords.Length * 2;
            }
        }

        var bank84 = new byte[0x8000];
        for (int index = 0; index < bank84.Length; index++)
            bank84[index] = rom.ReadByte(0x848000 + index);
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
            VerifyMotherBrainGlassNativeDrawPath(bank84, lists, list,
                list.Pointer == 0x978f ? edited : null);
        AssertThrows<InvalidDataException>(
            () => new RoomPlmMotherBrainGlassVisualCatalog(entries.Skip(1)),
            "glass catalog rejects missing frames");
        shatter.Blocks[6] = 0xf057;
        AssertThrows<InvalidDataException>(
            () => new RoomPlmMotherBrainGlassVisualCatalog(entries),
            "glass catalog rejects collision bits in visual words");
        shatter.Blocks[6] = originalWord;
        VerifyMotherBrainGlassVisualInstallation(rom);
        Console.WriteLine(
            "  Mother Brain glass PLM: 11 guarded native layouts and editable stock/override appearance preserve physical blocks.");
    }

    private static void VerifyMotherBrainGlassNativeDrawPath(
        byte[] bank84,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists,
        RoomPlmShotBlockDrawDefinitions.DrawList selected,
        RoomPlmMotherBrainGlassVisualCatalog? visuals)
    {
        const int width = 32;
        const int height = 16;
        const int originX = 9;
        const int originY = 5;
        var bus = new TestAddressSpace();
        bus.WriteBytes(0x848000, bank84);
        bus.WriteBytes(0x8f9000,
        [
            unchecked((byte)RoomPlmHeaders.MotherBrainGlass),
            unchecked((byte)(RoomPlmHeaders.MotherBrainGlass >> 8)),
            originX, originY, 0x00, 0x80, 0x00, 0x00,
        ]);
        // The production control list is compiled and immutable. Redirect the
        // loaded glass slot to an isolated one-frame draw probe instead of
        // mutating a cartridge operand that execution no longer reads.
        const ushort probeList = 0xf100;
        WriteWord(bus, 0x840000 | probeList, 1);
        WriteWord(bus, 0x840000 | (probeList + 2), selected.Pointer);
        var guarded = new MotherBrainGlassDrawReadGuard(bus, lists);
        byte[] blockDefinitions = new byte[0x400 * 8];
        blockDefinitions[0x57 * 8] = 0x57;
        var level = new RoomLevelData(width, height,
            new ushort[width * height], new byte[width * height],
            new ushort[width * height], blockDefinitions);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var plms = new RoomPlmSystem { MotherBrainGlassVisuals = visuals };
        AssertEqual(1, plms.LoadRoomPopulation(guarded, level, streamer,
                new SnesVram(), 0x9000, new Bank80SystemState(), AreaId.Tourian,
                () => new SamusState(), () => false,
                hasAreaBossBit: _ => false,
                hasEvent: _ => false,
                setEvent: _ => { }),
            $"Mother Brain glass loads for draw ${selected.Pointer:X4}");
        AssertTrue(plms.MotherBrainGlassWasLoaded,
            $"glass header retains its PLM owner for draw ${selected.Pointer:X4}");
        plms.SetSoleInstructionPointerForVerification(probeList);
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
                $"glass draw ${selected.Pointer:X4} writes native physical block {index}");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"glass draw ${selected.Pointer:X4} avoids source payload reads");
        if (visuals is not null)
        {
            const int editedBlock = 7 * width + 6;
            AssertTrue(plms.TilemapUpdates.Any(update => update.TopRow[0] == 0x0057),
                "edited multi-run glass tile reaches the immediate PLM update");
            AssertEqual((ushort)0x0057,
                level.CreateBackgroundStreamer()
                    .BuildPlmLevelBlockUpdate(editedBlock, 0).TopRow[0],
                "edited glass tile survives later camera streaming");
            AssertEqual((ushort)0x0ecf,
                level.GetCollisionBlockByIndex(editedBlock).LevelWord,
                "edited glass appearance does not change the physical shatter block");
        }
    }

    private static void VerifyMotherBrainGlassVisualInstallation(
        SuperMetroidAddressSpace rom)
    {
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "mother-brain-glass-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Glass visual test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmMotherBrainGlassVisualFiles.Extract(rom,
                installation.RoomPlmMotherBrainGlassVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmMotherBrainGlassVisualFiles.ValidateStock(
                installation.RoomPlmMotherBrainGlassVisualDirectory);
            string stockPath = Path.Combine(
                installation.RoomPlmMotherBrainGlassVisualDirectory,
                RoomPlmMotherBrainGlassVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted glass JSON is empty.");
            JsonNode frame = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() == "shatter-1")!;
            frame["blocks"]![6] = 0x0057;
            Directory.CreateDirectory(
                installation.RoomPlmMotherBrainGlassVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmMotherBrainGlassVisualOverrideDirectory,
                RoomPlmMotherBrainGlassVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0057,
                installation.LoadRoomPlmMotherBrainGlassVisuals()
                    .GetWord(0x978f, 1, 2),
                "installed glass override changes the multi-run shatter tile");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmMotherBrainGlassVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0057,
                RoomPlmMotherBrainGlassVisualFiles.Load(refreshed,
                    installation.RoomPlmMotherBrainGlassVisualOverrideDirectory)
                    .GetWord(0x978f, 1, 2),
                "glass override survives stock replacement");

            frame["blocks"]![6] = 0xf057;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmMotherBrainGlassVisuals(),
                "invalid glass override fails loudly");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmMotherBrainGlassVisuals(),
                "tampered glass stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    private sealed class MotherBrainGlassDrawReadGuard(
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
                        $"Mother Brain glass reread draw payload ${address:X6}.");
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
