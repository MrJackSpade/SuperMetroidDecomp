using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Abilities.SpeedBoosterCollisionBlocksVerification;
using static SuperMetroid.Verification.Samus.Abilities.SpeedBoosterPlmProgramsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;

/// <summary>Checks the bomb-revealed Speed Booster block's editable image independently of its collision behavior.</summary>
internal static class SpeedBoosterVisualsVerification
{
    /// <summary>Checks native visual identity, exact pointer membership, authored shape, and source-array isolation.</summary>
    private static void VerifySpeedBoosterStockVisualMapping(SuperMetroidAddressSpace rom)
    {
        const ushort pointer = 0xa4f3;
        ushort native = (ushort)(ReadRomWord(rom,0x84a4f5) & 0xfff);
        var stock = RoomPlmSpeedBoosterVisualCatalog.Stock();
        var imported = new RoomPlmSpeedBoosterVisualCatalog([new("bomb-reveal",[native])]);
        string Hash(ushort word) => SuperMetroid.Core.Assets.SelectedPresentationHash.Create(
            nameof(RoomPlmSpeedBoosterVisualCatalog), content => content.Append("visual word",word));
        AssertEqual(native,stock.GetWord(NativePlmDraws.At<SpeedBoosterBlockPlmDrawDefinitions.Draw>(pointer),0,0),"speed stock visual matches original native draw");
        AssertEqual(native,imported.GetWord(NativePlmDraws.At<SpeedBoosterBlockPlmDrawDefinitions.Draw>(pointer),0,0),"speed imported visual matches original native draw");
        AssertEqual(Hash(native),stock.ContentIdentity,"speed stock scalar identity");
        AssertEqual(Hash(native),imported.ContentIdentity,"speed imported scalar identity");
        for (int address = 0; address <= ushort.MaxValue; address++)
            if (address != pointer)
                AssertThrows<InvalidDataException>(() => stock.GetWord(NativePlmDraws.At<SpeedBoosterBlockPlmDrawDefinitions.Draw>((ushort)address),0,0),"speed visual full pointer rejection");
        foreach (int invalid in new[] {int.MinValue,-1,1,2,int.MaxValue})
        {
            AssertThrows<InvalidDataException>(() => stock.GetWord(NativePlmDraws.At<SpeedBoosterBlockPlmDrawDefinitions.Draw>(pointer),invalid,0),"speed visual run bounds");
            AssertThrows<InvalidDataException>(() => stock.GetWord(NativePlmDraws.At<SpeedBoosterBlockPlmDrawDefinitions.Draw>(pointer),0,invalid),"speed visual block bounds");
        }
        ushort[] custom = [0x0453];
        var edited = new RoomPlmSpeedBoosterVisualCatalog([new("bomb-reveal",custom)]);
        custom[0] = 0x0054;
        AssertEqual((ushort)0x0453,edited.GetWord(NativePlmDraws.At<SpeedBoosterBlockPlmDrawDefinitions.Draw>(pointer),0,0),"speed visual scalar isolates source-array edits");
        AssertEqual(Hash(0x0453),edited.ContentIdentity,"speed custom scalar identity including flip bits");
        AssertThrows<InvalidDataException>(() => new RoomPlmSpeedBoosterVisualCatalog([]),"speed visual requires entry");
        AssertThrows<InvalidDataException>(() => new RoomPlmSpeedBoosterVisualCatalog([new("bomb-reveal",[native]),new("bomb-reveal",[native])]),"speed visual rejects duplicate entry");
        AssertThrows<InvalidDataException>(() => new RoomPlmSpeedBoosterVisualCatalog([new("Bomb-reveal",[native])]),"speed visual ID is ordinal");
        AssertThrows<InvalidDataException>(() => new RoomPlmSpeedBoosterVisualCatalog([new("bomb-reveal",[0xb0b6])]),"speed visual rejects collision bits");
        AssertThrows<InvalidDataException>(() => new RoomPlmSpeedBoosterVisualCatalog([new("bomb-reveal",[native,native])]),"speed visual rejects altered shape");
    }

    /// <summary>Checks extraction and a production bomb reveal with immediate and streamed art but unchanged physical block.</summary>
    internal static void VerifySpeedBoosterVisuals()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifySpeedBoosterStockVisualMapping), () => VerifySpeedBoosterStockVisualMapping(rom));
        using var testRootScratch = new TestScratchDirectory("speed-booster-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmSpeedBoosterVisualFiles.Extract(rom,
                installation.RoomPlmSpeedBoosterVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmSpeedBoosterVisualFiles.ValidateStock(
                installation.RoomPlmSpeedBoosterVisualDirectory);
            PlmDraw drawPointer = SpeedBoosterBlockPlmDrawDefinitions.BombRevealDraw;
            AssertEqual((ushort)0x00b6,
                installation.LoadRoomPlmSpeedBoosterVisuals()
                    .GetWord(drawPointer, 0, 0),
                "stock bomb-reveal visual matches the cartridge");

            string stockPath = Path.Combine(
                installation.RoomPlmSpeedBoosterVisualDirectory,
                RoomPlmSpeedBoosterVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted Speed Booster JSON is empty.");
            document.Required("entries").Required(0).Required("blocks")[0] = 0x0058;
            Directory.CreateDirectory(
                installation.RoomPlmSpeedBoosterVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmSpeedBoosterVisualOverrideDirectory,
                RoomPlmSpeedBoosterVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            RoomPlmSpeedBoosterVisualCatalog edited =
                installation.LoadRoomPlmSpeedBoosterVisuals();
            AssertEqual((ushort)0x0058, edited.GetWord(drawPointer, 0, 0),
                "Speed Booster visual override is loaded");

            (TestAddressSpace bus, RoomLevelData level, RoomPlmSystem plms,
                int blockIndex) = CreateSpeedBoosterBlockFixture(
                    new RoomBlockBehavior(0x82), AreaId.Brinstar);
            level.SetBlockDefinitionWord(0x0058 * 4, 0x0058);
            PlmFixtures.VisualsOf(plms).RoomPlmSpeedBoosterVisuals = edited;
            var guard = NewSpeedBoosterPlmReadGuard(bus);
            AssertTrue(plms.BlockReactions.TrySpawnBombedSpecialBlock(level, blockIndex,
                    new RoomBlockBehavior(0x82), AreaId.Brinstar, 0x0500),
                "bomb triggers the production Speed Booster reveal PLM");
            plms.Step(level);
            AssertEqual((ushort)0xb0b6,
                level.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw,
                "edited reveal retains compiled type-B physical level word");
            // A PLM redraw identifies its block by the BG1 ring destination of that block.
            AssertEqual((ushort)0x0058, level.DisplayedBlockTiles(blockIndex).TopLeft.Raw,
                "edited reveal reaches the immediate tilemap update");
            AssertEqual((ushort)0x0058,
                level.DisplayedBlockTiles(blockIndex).TopLeft.Raw,
                "edited reveal survives later camera streaming");
            plms.Step(level);
            AssertEqual(0, plms.SlotTable.ActiveCount,
                "edited reveal retains native one-frame deletion");
            AssertEqual(0, guard.ForbiddenReadAttempts,
                "edited Speed Booster reveal reads no compiled ROM source bytes");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmSpeedBoosterVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0058,
                RoomPlmSpeedBoosterVisualFiles.Load(refreshed,
                    installation.RoomPlmSpeedBoosterVisualOverrideDirectory)
                    .GetWord(drawPointer, 0, 0),
                "override survives stock replacement");
            document.Required("entries").Required(0).Required("blocks")[0] = 0xf058;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmSpeedBoosterVisuals(),
                "override cannot change collision bits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmSpeedBoosterVisualFiles.ValidateStock(
                    installation.RoomPlmSpeedBoosterVisualDirectory),
                "stock tampering fails validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
        Console.WriteLine(
            "Speed Booster visuals: native reveal parity, live edit, physical isolation, stock repair and strict failures pass.");
    }

    /// <summary>Runs the focused Speed Booster reveal artwork check against the retail ROM.</summary>
    [VerificationCommand("--speed-booster-visuals")]
    internal static int SpeedBoosterVisualsCommand()
    {
        Suite(nameof(VerifySpeedBoosterVisuals), () => VerifySpeedBoosterVisuals());
        return 0;
    }
}
