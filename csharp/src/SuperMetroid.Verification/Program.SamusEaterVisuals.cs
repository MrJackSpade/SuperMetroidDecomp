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
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
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
            RoomPlmSamusEaterVisualCatalog stock =
                installation.LoadRoomPlmSamusEaterVisuals();
            AssertEqual(8, SamusEaterPlmDrawDefinitions.All.Count(),
                "all eight floor/ceiling frames are compiled");
            foreach (var draw in SamusEaterPlmDrawDefinitions.All)
            {
                ushort cursor = draw.Pointer;
                for (int runIndex = 0; runIndex < draw.Runs.Length; runIndex++)
                {
                    var run = draw.Runs.Span[runIndex];
                    AssertEqual(run.DirectionAndCount,
                        ReadSamusEaterPlmWord(rom, 0x840000 | cursor),
                        "Samus Eater native draw direction/count");
                    for (int block = 0; block < run.LevelWords.Length; block++)
                    {
                        ushort native = ReadSamusEaterPlmWord(rom,
                            0x840000 | checked((ushort)(cursor + 2 + 2 * block)));
                        AssertEqual(native, run.LevelWords.Span[block],
                            "Samus Eater native physical block word");
                        AssertEqual(new RoomLevelWord(native).VisualWord,
                            stock.GetWord(draw.Pointer, runIndex, block),
                            "Samus Eater stock visual block word");
                    }
                    ushort offset = ReadSamusEaterPlmWord(rom,
                        0x840000 | checked((ushort)(cursor + 2 +
                            2 * run.LevelWords.Length)));
                    AssertEqual((ushort)(unchecked((byte)run.NextX) |
                        unchecked((byte)run.NextY) << 8), offset,
                        "Samus Eater native signed run offset");
                    cursor = checked((ushort)(cursor + 4 +
                        2 * run.LevelWords.Length));
                }
            }

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
            VerifySamusEaterLiveDraw(edited,
                SamusEaterPlmDrawDefinitions.FloorChew2, 0x05a5, 0x0058);
            VerifySamusEaterLiveDraw(edited,
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

    private static void VerifySamusEaterLiveDraw(
        RoomPlmSamusEaterVisualCatalog visuals, ushort pointer,
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
            .Single(method => method.Name == "DrawRomInstruction" &&
                method.GetParameters().Length == 9);
        int origin = 8 * width + 8;
        draw.Invoke(plms, [guard, level, streamer, (ushort)0, origin, pointer,
            (ushort)0, (ushort)0, (ushort)0]);
        AssertEqual(physicalWord,
            level.GetCollisionBlockByIndex(origin).LevelWord,
            "Samus Eater edited draw retains native collision word");
        AssertTrue(plms.TilemapUpdates.Any(update =>
                update.BlockIndex == origin && update.TopRow[0] == visualWord),
            "Samus Eater edited block reaches the real redraw path");
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
