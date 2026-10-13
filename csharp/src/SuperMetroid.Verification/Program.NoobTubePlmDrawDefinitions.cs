using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Spatial;
using static AssertionsVerification;
using static LookupNoobTubeStockVerification;
using static LookupTubeDrawsVerification;
using static SuiteTimingVerification;
using static SyntheticRoomCartridgeFixturesVerification;

/// <summary>Checks Noob Tube draw geometry and editable visual tiles through production PLM drawing.</summary>
internal static class NoobTubePlmDrawDefinitionsVerification
{
    /// <summary>Compares seven physical draw lists and checks that an edited tile leaves collision blocks intact.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Noob Tube PLM draw definitions")]
    internal static void VerifyNoobTubePlmDrawDefinitions(SuperMetroidAddressSpace rom)
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
            bank84[index] = rom.ReadCartridgeByte(0x848000 + index);
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
            VerifyNoobTubeNativeDrawPath(bank84, lists, list,
                list.Pointer == 0x98e3 ? edited : null);
        Suite(nameof(VerifyNoobTubeVisualInstallation), () => VerifyNoobTubeVisualInstallation(rom));
        Console.WriteLine(
            "  N00b-tube PLM: seven guarded native layouts and editable stock/override appearance preserve physical blocks.");
    }

    /// <summary>Draws one native layout through the guarded PLM path and compares resulting blocks and streamed art.</summary>
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
        TestAddressSpace bus = CreateBank84Cartridge(bank84);
        SeedRoomPlmPopulation(bus, 0x9400, 
        [
            unchecked((byte)PlmHeaderId.NoobTube),
            SnesLittleEndian.HighByte((ushort)PlmHeaderId.NoobTube),
            originX, originY, 0, 0, 0, 0,
        ]);
        // The retail control stream is compiled and immutable. Draw each physical layout through
        // the production timed-frame draw without patching that catalog.
        var guarded = NewNoobTubeDrawReadGuard(bus, lists);
        byte[] blockDefinitions = new byte[0x400 * 8];
        blockDefinitions[0x59 * 8] = 0x59;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(new ushort[width * height], new byte[width * height], new ushort[width * height]), blockDefinitions);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var plms = new RoomPlmSystem { Drawing = { NoobTubeVisuals = visuals } };
        AssertEqual(1, plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(guarded, level, streamer, new SnesVram(), new Bank80SystemState(), AreaId.Maridia), RoomPlmPopulationImporter.Read(guarded, 0x9400), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Events = new FixtureEventFlags { HasEvent = _ => false, SetEvent = _ => { } } }),
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
            entryX = originX + run.Next.X;
            entryY = originY + run.Next.Y;
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

    /// <summary>Checks extracted stock, selected override, stock replacement, and invalid visual documents.</summary>
    private static void VerifyNoobTubeVisualInstallation(SuperMetroidAddressSpace rom)
    {
        using var testRootScratch = new TestScratchDirectory("noob-tube-visual");
        string testRoot = testRootScratch.Root;
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
            JsonNode frame = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "cleared");
            frame.Required("blocks")[24] = 0x0059;
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

            frame.Required("blocks")[24] = 0xf059;
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

    /// <summary>Rejects runtime reads of all seven compiled bank 84 draw payload ranges.</summary>
    private static CartridgeReadGuard NewNoobTubeDrawReadGuard(
        ISnesAddressSpace source, RoomPlmShotBlockDrawDefinitions.DrawList[] lists) =>
        new CartridgeReadGuard(source).Forbid(address => lists.Any(list =>
            {
                int first = 0x840000 | list.Pointer;
                int length = list.Runs.Span.ToArray().Sum(run =>
                    4 + run.LevelWords.Length * 2);
                return address >= first && address < first + length;
            }),
            address => $"N00b tube reread draw payload ${address:X6}.");
}
