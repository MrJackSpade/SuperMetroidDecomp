using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>
    /// Checks the Noob Tube draw definitions, physical collision geometry, visual catalog, and
    /// production draw path against the cartridge data.
    /// </summary>
    private static void VerifyNoobTubePlmDrawDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyTubeGeometry), () => VerifyTubeGeometry(rom));
        Suite(nameof(VerifyTubeCollision), () => VerifyTubeCollision(rom));
        Suite(nameof(VerifyTubeVisuals), () => VerifyTubeVisuals(rom));

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
        var bank84 = new byte[0x8000];
        for (int index = 0; index < bank84.Length; index++)
            bank84[index] = rom.ReadByte(0x848000 + index);
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
            VerifyNoobTubeNativeDrawPath(bank84, lists, list,
                list.Pointer == 0x98e3 ? edited : null);
        Suite(nameof(VerifyNoobTubeVisualInstallation), () => VerifyNoobTubeVisualInstallation(rom));
        Console.WriteLine(
            "  N00b-tube PLM: seven guarded native layouts and editable stock/override appearance preserve physical blocks.");
    }

    /// <summary>
    /// Runs one compiled tube draw list through the production PLM renderer and verifies its
    /// physical blocks while guarding the immutable draw payload against runtime reads.
    /// </summary>
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
        // The retail control stream is compiled and immutable. Draw each physical layout through
        // the production timed-frame draw without patching that catalog.
        var guarded = new NoobTubeDrawReadGuard(bus, lists);
        byte[] blockDefinitions = new byte[0x400 * 8];
        blockDefinitions[0x59 * 8] = 0x59;
        var level = new RoomLevelData(width, height,
            new ushort[width * height], new byte[width * height],
            new ushort[width * height], blockDefinitions);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var plms = new RoomPlmSystem { NoobTubeVisuals = visuals };
        AssertEqual(1, plms.LoadRoomPopulation(guarded, level, streamer,
                new SnesVram(), RoomPlmPopulationImporter.Read(guarded, 0x9400), new Bank80SystemState(), AreaId.Maridia,
                () => new SamusState(), () => false,
                hasEvent: _ => false,
                setEvent: _ => { }),
            $"n00b tube loads for draw ${selected.Pointer:X4}");
        plms.DrawSolePlmFrameForVerification(guarded, level, streamer, selected.Pointer);

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

    /// <summary>
    /// Exercises extraction, stock-manifest validation, visual overrides, stock replacement,
    /// and rejection of malformed tube visual data in an isolated installation directory.
    /// </summary>
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
            VerifyNoobTubeStockMapping(rom, installation.LoadRoomPlmNoobTubeVisuals());
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

    /// <summary>
    /// Wraps the test address space and fails if rendering reads bytes belonging to any compiled
    /// Noob Tube draw-list payload.
    /// </summary>
    /// <param name="source">Address space that supplies all non-draw-payload reads and receives writes.</param>
    /// <param name="lists">Compiled draw lists whose encoded source ranges must not be read during rendering.</param>
    private sealed class NoobTubeDrawReadGuard(
        ISnesAddressSpace source,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets the number of rejected reads into compiled draw-list payloads.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Reads a cartridge byte through the same guarded path used by the address-space interface.</summary>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>
        /// Returns a byte from the wrapped address space, throwing if the requested address lies
        /// inside one of the compiled draw-list payloads.
        /// </summary>
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

        /// <summary>Forwards a byte write to the wrapped address space.</summary>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
