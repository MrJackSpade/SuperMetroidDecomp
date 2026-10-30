using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.SporeSpawn.SporeSpawnCeilingPlmsVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Bosses.SporeSpawn;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

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
            first.Required("blocks")[0] = InstalledAppearanceJson.Cell(0x58);
            clear.Required("blocks")[3] = InstalledAppearanceJson.Cell(0x59);
            Directory.CreateDirectory(
                installation.RoomPlmSporeSpawnCeilingVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmSporeSpawnCeilingVisualOverrideDirectory,
                RoomPlmSporeSpawnCeilingVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            RoomPlmSporeSpawnCeilingVisualCatalog edited =
                installation.LoadRoomPlmSporeSpawnCeilingVisuals();
            AssertEqual(RoomBlockAppearance.Of(0x58),
                edited.GetAppearance(PlmDraw.Of(SporeSpawnCeilingDraw.CrumbleFirst),
                    0, 0),
                "Spore Spawn override edits the first crumble block");
            AssertEqual(RoomBlockAppearance.Of(0x59),
                edited.GetAppearance(PlmDraw.Of(SporeSpawnCeilingDraw.Clear),
                    1, 1),
                "Spore Spawn override edits the bottom-right clear block");

            VerifySporeSpawnCeilingVisualSeparation(edited, clear: false);
            VerifySporeSpawnCeilingVisualSeparation(edited, clear: true);

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmSporeSpawnCeilingVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual(RoomBlockAppearance.Of(0x59),
                RoomPlmSporeSpawnCeilingVisualFiles.Load(refreshed,
                    installation.RoomPlmSporeSpawnCeilingVisualOverrideDirectory)
                    .GetAppearance(PlmDraw.Of(SporeSpawnCeilingDraw.Clear), 1, 1),
                "Spore Spawn override survives stock replacement");

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

    /// <summary>Compares flattened stock appearances with the decoded cartridge draws and checks catalog identity, shape, and bounds.</summary>
    private static void VerifySporeSpawnCeilingStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmSporeSpawnCeilingVisualCatalog installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x9413,"clear-ceiling"), new LookupEscapeGateStockFrame(0x9423,"crumble-frame-0"),
            new LookupEscapeGateStockFrame(0x9433,"crumble-frame-1"), new LookupEscapeGateStockFrame(0x9443,"crumble-frame-2")];
        var native = new Dictionary<ushort, RoomBlockAppearance[]>();
        foreach (var frame in frames)
        {
            var appearances = new RoomBlockAppearance[4];
            for (int index = 0; index < appearances.Length; index++)
                appearances[index] = RoomLevelWordCodec.Decode(ReadSamusEaterPlmWord(rom,
                    0x840000 | (frame.Pointer + index / 2 * 8 + index % 2 * 2 + 2))).Appearance;
            native.Add(frame.Pointer, appearances);
        }
        var entries = frames.Select(frame => new RoomPlmVisualEntry(frame.Id, native[frame.Pointer].ToArray())).ToArray();
        var stock = RoomPlmSporeSpawnCeilingVisualCatalog.Stock();
        var imported = new RoomPlmSporeSpawnCeilingVisualCatalog(entries.Reverse());
        // Hash flattened visual frames by native draw pointer for catalog identity comparison.
        string Hash(Dictionary<ushort, RoomBlockAppearance[]> appearances) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromAppearanceFrames(
            nameof(RoomPlmSporeSpawnCeilingVisualCatalog), appearances.ToDictionary(pair => SporeSpawnCeilingDraws.VisualId(NativePlmDraws.At<SporeSpawnCeilingDraw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock, imported, installed})
        {
            AssertEqual(Hash(native), catalog.ContentIdentity, "Spore ceiling original flattened hash");
            foreach (var frame in frames)
            {
                for (int index = 0; index < 4; index++)
                    AssertEqual(native[frame.Pointer][index], catalog.GetAppearance(NativePlmDraws.At<SporeSpawnCeilingDraw>(frame.Pointer),index / 2,index % 2), "Spore ceiling native stock word");
                foreach (int bad in new[] {int.MinValue,-1,2,int.MaxValue})
                {
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<SporeSpawnCeilingDraw>(frame.Pointer),bad,0), "Spore ceiling run bounds");
                    for (int run = 0; run < 2; run++)
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<SporeSpawnCeilingDraw>(frame.Pointer),run,bad), "Spore ceiling block bounds");
                }
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativePlmDraws.At<SporeSpawnCeilingDraw>((ushort)pointer),0,0), "Spore ceiling unknown pointer");
        entries[0].Blocks[3] = RoomBlockAppearance.Of(0x58, BlockMirroring.Horizontal | BlockMirroring.Vertical);
        entries[2].Blocks[0] = RoomBlockAppearance.Of(0x59);
        var mixed = new RoomPlmSporeSpawnCeilingVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer, frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Spore ceiling mixed custom stock hash");
        entries[0].Blocks[3] = RoomBlockAppearance.Of(0x5a);
        entries[1].Blocks[0] = RoomBlockAppearance.Of(0x5b);
        foreach (var frame in frames)
        for (int index = 0; index < 4; index++)
            AssertEqual(expected[frame.Pointer][index], mixed.GetAppearance(NativePlmDraws.At<SporeSpawnCeilingDraw>(frame.Pointer),index / 2,index % 2), "Spore ceiling clone isolation and stock fallback");
        AssertThrows<InvalidDataException>(() => new RoomPlmSporeSpawnCeilingVisualCatalog(entries[..3]), "Spore ceiling missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmSporeSpawnCeilingVisualCatalog(entries.Append(entries[0])), "Spore ceiling duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("CLEAR-CEILING", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => new RoomPlmSporeSpawnCeilingVisualCatalog(invalid), "Spore ceiling ordinal identity");
        invalid[0] = new(entries[0].Id, new RoomBlockAppearance[3]);
        AssertThrows<InvalidDataException>(() => new RoomPlmSporeSpawnCeilingVisualCatalog(invalid), "Spore ceiling exact frame shape");
    }

    /// <summary>Runs a clear or crumble ceiling PLM and checks edited redraws without changing collision or deletion timing.</summary>
    private static void VerifySporeSpawnCeilingVisualSeparation(
        RoomPlmSporeSpawnCeilingVisualCatalog edited, bool clear)
    {
        const int width = 16;
        const int height = 32;
        var terrain = new RoomTerrainBlock[width * height];
        terrain.AsSpan().Fill(RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.Air));
        for (int y = 30; y <= 31; y++)
        for (int x = 7; x <= 8; x++)
            terrain[y * width + x] = RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.Air);
        RoomLevelData level = CreateRoom(new(width, height), terrain, new RoomBlockBehavior[terrain.Length]);
        var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmSporeSpawnCeilingVisuals = edited });
        AssertTrue(plms.BossRoomMutations.TrySpawnSporeSpawnCeiling(level,
                clear ? PlmHeaderId.ClearSporeSpawnCeiling :
                    PlmHeaderId.CrumbleSporeSpawnCeiling),
            $"Spore Spawn {(clear ? "clear" : "crumble")} PLM allocates");
        var guard = NewSporeSpawnCeilingSourceGuard(new TestAddressSpace());
        plms.Step(level);
        int blockIndex = clear ? 31 * width + 8 : 30 * width + 7;
        RoomTerrainBlock expectedPhysical = RoomTerrainBlock.Of(clear ? 0xff : 0x53, BlockMirroring.None, RoomCollisionType.Air);
        RoomBlockAppearance expectedVisual = RoomBlockAppearance.Of(clear ? 0x59 : 0x58);
        AssertEqual(expectedPhysical,
            level.GetCollisionBlockByIndex(blockIndex).Terrain,
            $"Spore Spawn {(clear ? "clear" : "crumble")} edited art retains physical block");
        // The redraw identifies its block by the block's DrawPLM BG1 ring destination.
        AssertEqual(expectedVisual, level.DisplayedAppearance(blockIndex),
            $"Spore Spawn {(clear ? "clear" : "crumble")} edit reaches immediate redraw");
        AssertEqual(expectedVisual,
            level.DisplayedAppearance(blockIndex),
            $"Spore Spawn {(clear ? "clear" : "crumble")} edit survives streaming");
        int deletionFrame = -1;
        for (int frame = 1; frame < 24 && plms.SlotTable.ActiveCount != 0; frame++)
        {
            plms.Step(level);
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
