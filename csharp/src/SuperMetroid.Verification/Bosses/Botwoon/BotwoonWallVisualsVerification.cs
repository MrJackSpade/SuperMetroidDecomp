using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Botwoon.BotwoonWallPlmsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks editable Botwoon wall-clear tiles remain separate from PLM collision and timing.</summary>
internal static class BotwoonWallVisualsVerification
{
    /// <summary>Compares the nine stock draw words with cartridge data and checks cloned custom artwork.</summary>
    internal static void VerifyBotwoonWallStockMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] native = new ushort[9];
        for (int index = 0; index < native.Length; index++)
            native[index] = (ushort)(ReadRomWord(rom, 0x849311 + 2 * index) & 0x0fff);
        var stock = RoomPlmBotwoonWallVisualCatalog.Stock();
        var loaded = new RoomPlmBotwoonWallVisualCatalog([new("clear-wall", native)]);
        string expectedIdentity = SuperMetroid.Core.Assets.SelectedPresentationHash.Create(
            nameof(RoomPlmBotwoonWallVisualCatalog), content => content.AppendWords("blocks", native));
        AssertEqual(expectedIdentity, stock.ContentIdentity, "Botwoon calculated stock preserves prior content identity");
        AssertEqual(expectedIdentity, loaded.ContentIdentity, "Botwoon loaded stock identity");
        ushort[] customWords = [0x10,0x20,0x30,0x58,0x50,0x60,0x70,0x80,0x90];
        var custom = new RoomPlmBotwoonWallVisualCatalog([new("clear-wall", customWords)]);
        string customIdentity = custom.ContentIdentity;
        for (int index = 0; index < native.Length; index++)
        {
            AssertEqual(native[index], stock.GetWord(NativePlmDraws.At<BotwoonWallDraw>(0x930f), 0, index), "Botwoon stock appearance matches native");
            AssertEqual(native[index], loaded.GetWord(NativePlmDraws.At<BotwoonWallDraw>(0x930f), 0, index), "Botwoon imported stock appearance matches native");
            AssertEqual(customWords[index], custom.GetWord(NativePlmDraws.At<BotwoonWallDraw>(0x930f), 0, index), "Botwoon custom appearance preserved");
        }
        Array.Fill(customWords, (ushort)0);
        AssertEqual(customIdentity, custom.ContentIdentity, "Botwoon custom payload remains cloned");
        AssertTrue(customIdentity != expectedIdentity, "Botwoon custom content retains distinct identity");
        foreach (var catalog in new[] {stock, loaded, custom})
        {
            foreach (int index in new[] {int.MinValue,-1,9,int.MaxValue})
                AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(NativePlmDraws.At<BotwoonWallDraw>(0x930f), 0, index), "Botwoon visual block domain");
            foreach (int run in new[] {int.MinValue,-1,1,int.MaxValue})
                AssertThrows<InvalidDataException>(() => catalog.GetWord(NativePlmDraws.At<BotwoonWallDraw>(0x930f), run, 0), "Botwoon visual run domain");
            foreach (ushort pointer in new ushort[] {0,0x930e,0x9310,0xffff})
                AssertThrows<InvalidDataException>(() => catalog.GetWord(NativePlmDraws.At<BotwoonWallDraw>(pointer), 0, 0), "Botwoon visual pointer domain");
        }
        Suite(nameof(VerifyBotwoonWallVisualSeparation), () => VerifyBotwoonWallVisualSeparation(custom));
    }

    /// <summary>Requires only the native clear-draw pointer to resolve to Botwoon's stable visual ID.</summary>
    internal static void VerifyBotwoonWallVisualIdMapping()
    {
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (pointer == 0x930f)
                AssertEqual("clear-wall", BotwoonWallPlmDrawDefinitions.VisualId(NativePlmDraws.At<BotwoonWallDraw>((ushort)pointer)), "Botwoon stable export identity");
            else
                AssertThrows<InvalidDataException>(() => BotwoonWallPlmDrawDefinitions.VisualId(NativePlmDraws.At<BotwoonWallDraw>((ushort)pointer)), "Botwoon visual ID accepts only the clear draw");
    }

    /// <summary>Checks extraction, installed overrides, stock repair, and rejection of physical-bit edits.</summary>
    internal static void VerifyBotwoonWallVisuals()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        using var testRootScratch = new TestScratchDirectory("botwoon-wall-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmBotwoonWallVisualFiles.Extract(rom,
                installation.RoomPlmBotwoonWallVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmBotwoonWallVisualFiles.ValidateStock(
                installation.RoomPlmBotwoonWallVisualDirectory);
            RoomPlmBotwoonWallVisualCatalog stock =
                installation.LoadRoomPlmBotwoonWallVisuals();
            for (int block = 0; block < 9; block++)
                AssertEqual((ushort)0x00ff,
                    stock.GetWord(BotwoonWallPlmDrawDefinitions.Clear,
                        0, block),
                    $"stock Botwoon wall clear block {block} matches ROM");
            AssertThrows<InvalidDataException>(
                () => new RoomPlmBotwoonWallVisualCatalog([]),
                "Botwoon wall visual catalog rejects missing clear frame");

            string stockPath = Path.Combine(
                installation.RoomPlmBotwoonWallVisualDirectory,
                RoomPlmBotwoonWallVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted Botwoon wall JSON is empty.");
            JsonNode clear = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "clear-wall");
            clear.Required("blocks")[3] = 0x0058;
            Directory.CreateDirectory(
                installation.RoomPlmBotwoonWallVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmBotwoonWallVisualOverrideDirectory,
                RoomPlmBotwoonWallVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            RoomPlmBotwoonWallVisualCatalog edited =
                installation.LoadRoomPlmBotwoonWallVisuals();
            AssertEqual((ushort)0x0058,
                edited.GetWord(BotwoonWallPlmDrawDefinitions.Clear, 0, 3),
                "Botwoon wall override edits the fourth clear block");
            VerifyBotwoonWallVisualSeparation(edited);

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmBotwoonWallVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0058,
                RoomPlmBotwoonWallVisualFiles.Load(refreshed,
                    installation.RoomPlmBotwoonWallVisualOverrideDirectory)
                    .GetWord(BotwoonWallPlmDrawDefinitions.Clear, 0, 3),
                "Botwoon wall override survives stock replacement");

            clear.Required("blocks")[3] = 0xf058;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmBotwoonWallVisuals(),
                "Botwoon wall override cannot change physical collision bits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmBotwoonWallVisualFiles.ValidateStock(
                    installation.RoomPlmBotwoonWallVisualDirectory),
                "tampered Botwoon wall stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
        Console.WriteLine(
            "Botwoon wall visuals: native nine-tile clear, live art edit, physical/timing isolation, stock repair and strict failures pass.");
    }

    /// <summary>Checks an edited clear tile reaches immediate and later BG1 draws without changing collision or deletion.</summary>
    internal static void VerifyBotwoonWallVisualSeparation(
        RoomPlmBotwoonWallVisualCatalog edited)
    {
        const int width = 32;
        const int height = 16;
        ushort[] words = new ushort[width * height];
        for (int y = 4; y <= 12; y++)
            words[y * width + 15] = 0x0123;
        RoomLevelData level = CreateRoom(new(width, height), words,
            new byte[words.Length], blockDefinitions: new byte[0x400 * 8]);
        level.SetBlockDefinitionWord(0x0058 * 4, 0x0058);
        var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmBotwoonWallVisuals = edited });
        AssertTrue(plms.BossRoomMutations.TrySpawnBotwoonWall(level,
                PlmHeaderId.ClearBotwoonWall),
            "edited Botwoon wall-clear PLM allocates");
        var guard = NewBotwoonWallSourceGuard(new TestAddressSpace());
        plms.Step(level);
        int blockIndex = 7 * width + 15;
        AssertEqual((ushort)0x00ff,
            level.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw,
            "edited Botwoon wall art retains the physical clear word");
        // A PLM redraw identifies its block by the BG1 ring destination it targets.
        AssertEqual((ushort)0x0058, level.DisplayedBlockTiles(blockIndex).TopLeft.Raw,
            "Botwoon wall edit reaches immediate redraw");
        AssertEqual((ushort)0x0058,
            level.DisplayedBlockTiles(blockIndex).TopLeft.Raw,
            "Botwoon wall edit survives background streaming");
        plms.Step(level);
        AssertEqual(0, plms.SlotTable.ActiveCount,
            "edited Botwoon wall preserves native deletion timing");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "edited Botwoon wall draw reads no source bytes");
    }

    /// <summary>Runs the installed Botwoon wall visual check from the command dispatcher.</summary>
    [VerificationCommand("--botwoon-wall-visuals")]
    internal static int BotwoonWallVisualsCommand()
    {
        Suite(nameof(VerifyBotwoonWallVisuals), () => VerifyBotwoonWallVisuals());
        return 0;
    }
}
