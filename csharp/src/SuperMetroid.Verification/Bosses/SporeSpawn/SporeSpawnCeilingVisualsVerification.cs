using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.SporeSpawn.SporeSpawnCeilingPlmsVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.SporeSpawn;
using SuperMetroid.Verification.Harness;

/// <summary>Checks editable Spore Spawn ceiling artwork while retaining compiled PLM physics and timing.</summary>
internal static class SporeSpawnCeilingVisualsVerification
{
    /// <summary>Checks four stock frames, two edited two-by-two draws, override persistence, and strict asset rejection.</summary>
    internal static void VerifySporeSpawnCeilingVisuals()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Spore ceiling stock oracle revision");
        using var testRootScratch = new TestScratchDirectory("spore-ceiling-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmSporeSpawnCeilingVisualFiles.Extract(rom,
                installation.RoomPlmSporeSpawnCeilingVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmSporeSpawnCeilingVisualFiles.ValidateStock(
                installation.RoomPlmSporeSpawnCeilingVisualDirectory);
            VerifySporeSpawnCeilingStockMapping(rom, installation.LoadRoomPlmSporeSpawnCeilingVisuals());

            string stockPath = Path.Combine(
                installation.RoomPlmSporeSpawnCeilingVisualDirectory,
                RoomPlmSporeSpawnCeilingVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted Spore Spawn ceiling JSON is empty.");
            JsonNode first = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "crumble-frame-0");
            JsonNode clear = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "clear-ceiling");
            first.Required("blocks")[0] = 0x0058;
            clear.Required("blocks")[3] = 0x0059;
            Directory.CreateDirectory(
                installation.RoomPlmSporeSpawnCeilingVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmSporeSpawnCeilingVisualOverrideDirectory,
                RoomPlmSporeSpawnCeilingVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            RoomPlmSporeSpawnCeilingVisualCatalog edited =
                installation.LoadRoomPlmSporeSpawnCeilingVisuals();
            AssertEqual((ushort)0x0058,
                edited.GetWord((ushort)SporeSpawnCeilingDraw.CrumbleFirst,
                    0, 0),
                "Spore Spawn override edits the first crumble block");
            AssertEqual((ushort)0x0059,
                edited.GetWord((ushort)SporeSpawnCeilingDraw.Clear,
                    1, 1),
                "Spore Spawn override edits the bottom-right clear block");

            VerifySporeSpawnCeilingVisualSeparation(edited, clear: false);
            VerifySporeSpawnCeilingVisualSeparation(edited, clear: true);

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmSporeSpawnCeilingVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0059,
                RoomPlmSporeSpawnCeilingVisualFiles.Load(refreshed,
                    installation.RoomPlmSporeSpawnCeilingVisualOverrideDirectory)
                    .GetWord((ushort)SporeSpawnCeilingDraw.Clear, 1, 1),
                "Spore Spawn override survives stock replacement");

            first.Required("blocks")[0] = 0xf058;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmSporeSpawnCeilingVisuals(),
                "Spore Spawn override cannot change physical collision bits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmSporeSpawnCeilingVisualFiles.ValidateStock(
                    installation.RoomPlmSporeSpawnCeilingVisualDirectory),
                "tampered Spore Spawn ceiling stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
        Console.WriteLine(
            "Spore Spawn ceiling visuals: four native frames, live 2x2 edits, physical/timing isolation, stock repair and strict failures pass.");
    }

    /// <summary>Compares flattened stock visual words with cartridge draws and checks catalog identity, shape, and bounds.</summary>
    private static void VerifySporeSpawnCeilingStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmSporeSpawnCeilingVisualCatalog installed)
    {
        (ushort Pointer, string Id)[] frames = [(0x9413,"clear-ceiling"), (0x9423,"crumble-frame-0"),
            (0x9433,"crumble-frame-1"), (0x9443,"crumble-frame-2")];
        var native = new Dictionary<ushort, ushort[]>();
        foreach (var frame in frames)
        {
            var words = new ushort[4];
            for (int index = 0; index < words.Length; index++)
                words[index] = (ushort)(ReadSamusEaterPlmWord(rom,
                    0x840000 | (frame.Pointer + index / 2 * 8 + index % 2 * 2 + 2)) & 0xfff);
            native.Add(frame.Pointer, words);
        }
        var entries = frames.Select(frame => new RoomPlmSporeSpawnCeilingVisualEntry(frame.Id, native[frame.Pointer].ToArray())).ToArray();
        var stock = RoomPlmSporeSpawnCeilingVisualCatalog.Stock();
        var imported = new RoomPlmSporeSpawnCeilingVisualCatalog(entries.Reverse());
        // Hash flattened visual frames by native draw pointer for catalog identity comparison.
        string Hash(Dictionary<ushort, ushort[]> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(
            nameof(RoomPlmSporeSpawnCeilingVisualCatalog), words);
        foreach (var catalog in new[] {stock, imported, installed})
        {
            AssertEqual(Hash(native), catalog.ContentIdentity, "Spore ceiling original flattened hash");
            foreach (var frame in frames)
            {
                for (int index = 0; index < 4; index++)
                    AssertEqual(native[frame.Pointer][index], catalog.GetWord(frame.Pointer,index / 2,index % 2), "Spore ceiling native stock word");
                foreach (int bad in new[] {int.MinValue,-1,2,int.MaxValue})
                {
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,bad,0), "Spore ceiling run bounds");
                    for (int run = 0; run < 2; run++)
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,run,bad), "Spore ceiling block bounds");
                }
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetWord((ushort)pointer,0,0), "Spore ceiling unknown pointer");
        entries[0].Blocks[3] = 0x0c58;
        entries[2].Blocks[0] = 0x0059;
        var mixed = new RoomPlmSporeSpawnCeilingVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer, frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Spore ceiling mixed custom stock hash");
        entries[0].Blocks[3] = 0x005a;
        entries[1].Blocks[0] = 0x005b;
        foreach (var frame in frames)
        for (int index = 0; index < 4; index++)
            AssertEqual(expected[frame.Pointer][index], mixed.GetWord(frame.Pointer,index / 2,index % 2), "Spore ceiling clone isolation and stock fallback");
        AssertThrows<InvalidDataException>(() => new RoomPlmSporeSpawnCeilingVisualCatalog(entries[..3]), "Spore ceiling missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmSporeSpawnCeilingVisualCatalog(entries.Append(entries[0])), "Spore ceiling duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("CLEAR-CEILING", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => new RoomPlmSporeSpawnCeilingVisualCatalog(invalid), "Spore ceiling ordinal identity");
        invalid[0] = new(entries[0].Id, new ushort[3]);
        AssertThrows<InvalidDataException>(() => new RoomPlmSporeSpawnCeilingVisualCatalog(invalid), "Spore ceiling exact frame shape");
        entries[0].Blocks[0] = 0xf058;
        AssertThrows<InvalidDataException>(() => new RoomPlmSporeSpawnCeilingVisualCatalog(entries), "Spore ceiling visual bits only");
    }

    /// <summary>Runs a clear or crumble ceiling PLM and checks edited redraws without changing collision or deletion timing.</summary>
    private static void VerifySporeSpawnCeilingVisualSeparation(
        RoomPlmSporeSpawnCeilingVisualCatalog edited, bool clear)
    {
        const int width = 16;
        const int height = 32;
        ushort[] words = new ushort[width * height];
        for (int y = 30; y <= 31; y++)
        for (int x = 7; x <= 8; x++)
            words[y * width + x] = 0x0123;
        RoomLevelData level = CreateRoom(new(width, height), words,
            new byte[words.Length], blockDefinitions: new byte[0x400 * 8]);
        level.SetBlockDefinitionWord(0x0058 * 4, 0x0058);
        level.SetBlockDefinitionWord(0x0059 * 4, 0x0059);
        var plms = new RoomPlmSystem { Drawing = { SporeSpawnCeilingVisuals = edited } };
        AssertTrue(plms.BossRoomMutations.TrySpawnSporeSpawnCeiling(level,
                clear ? PlmHeaderId.ClearSporeSpawnCeiling :
                    PlmHeaderId.CrumbleSporeSpawnCeiling),
            $"Spore Spawn {(clear ? "clear" : "crumble")} PLM allocates");
        var guard = NewSporeSpawnCeilingSourceGuard(new TestAddressSpace());
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        plms.Step(guard, level, streamer, new PlmView { Layer1Position = new(0, 16 * 16), Bg1XOffset = 0 });
        int blockIndex = clear ? 31 * width + 8 : 30 * width + 7;
        ushort expectedPhysical = clear ? (ushort)0x00ff : (ushort)0x0053;
        ushort expectedVisual = clear ? (ushort)0x0059 : (ushort)0x0058;
        AssertEqual(expectedPhysical,
            level.GetCollisionBlockByIndex(blockIndex).LevelWord,
            $"Spore Spawn {(clear ? "clear" : "crumble")} edited art retains physical word");
        // The redraw identifies its block by the block's DrawPLM BG1 ring destination.
        ushort blockDestination = level.CreateBackgroundStreamer()
            .BuildPlmLevelBlockUpdate(blockIndex, 0).TopRowDestination;
        AssertTrue(plms.TilemapUpdates.Any(update =>
                update.TopRowDestination == blockDestination &&
                update.TopRow[0] == expectedVisual),
            $"Spore Spawn {(clear ? "clear" : "crumble")} edit reaches immediate redraw");
        AssertEqual(expectedVisual,
            level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(
                blockIndex, 0).TopRow[0],
            $"Spore Spawn {(clear ? "clear" : "crumble")} edit survives streaming");
        int deletionFrame = -1;
        for (int frame = 1; frame < 24 && plms.SlotTable.ActiveCount != 0; frame++)
        {
            plms.Step(guard, level, streamer, new PlmView { Layer1Position = new(0, 16 * 16), Bg1XOffset = 0 });
            if (plms.SlotTable.ActiveCount == 0)
                deletionFrame = frame;
        }
        AssertEqual(clear ? 4 : 16, deletionFrame,
            $"Spore Spawn {(clear ? "clear" : "crumble")} edited art preserves deletion timing");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Spore Spawn {(clear ? "clear" : "crumble")} edited draw reads no source bytes");
    }

    /// <summary>Runs stock and edited Spore Spawn ceiling visual verification.</summary>
    [VerificationCommand("--spore-spawn-ceiling-visuals")]
    internal static int SporeSpawnCeilingVisualsCommand()
    {
        Suite(nameof(VerifySporeSpawnCeilingVisuals), () => VerifySporeSpawnCeilingVisuals());
        return 0;
    }
}
