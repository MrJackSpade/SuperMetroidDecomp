using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using static SuperMetroid.Verification.Areas.Maridia.LookupElevatubeStockVerification;
using static SuperMetroid.Verification.Areas.Maridia.MaridiaElevatubePlmVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Areas.Maridia;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks editable Maridia elevatube tile art through stock extraction, installation, and live PLM drawing.</summary>
internal static class MaridiaElevatubeVisualsVerification
{
    /// <summary>Confirms an art override reaches immediate and streamed tiles without changing collision, sound timing, or PLM deletion.</summary>
    internal static void VerifyMaridiaElevatubeVisuals()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Elevatube stock oracle revision");
        using var testRootScratch = new TestScratchDirectory("maridia-elevatube-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmMaridiaElevatubeVisualFiles.Extract(rom,
                installation.RoomPlmMaridiaElevatubeVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmMaridiaElevatubeVisualFiles.ValidateStock(
                installation.RoomPlmMaridiaElevatubeVisualDirectory);
            VerifyElevatubeStockMapping(rom, installation.LoadRoomPlmMaridiaElevatubeVisuals());

            string stockPath = Path.Combine(
                installation.RoomPlmMaridiaElevatubeVisualDirectory,
                RoomPlmMaridiaElevatubeVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted elevatube JSON is empty.");
            document.Required("entries").Required(0).Required("blocks")[0] = 0x0058;
            Directory.CreateDirectory(
                installation.RoomPlmMaridiaElevatubeVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmMaridiaElevatubeVisualOverrideDirectory,
                RoomPlmMaridiaElevatubeVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            RoomPlmMaridiaElevatubeVisualCatalog edited =
                installation.LoadRoomPlmMaridiaElevatubeVisuals();
            AssertEqual((ushort)0x0058,
                edited.GetWord(MaridiaElevatubePlmDefinitions.DrawPointer, 0, 0),
                "elevatube override replaces only the visible block");

            RoomLevelData level = CreateRoom(new(4, 4), new ushort[16], new byte[16],
                blockDefinitions: new byte[0x400 * 8]);
            level.SetBlockDefinitionWord(0x0058 * 4, 0x0058);
            var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmMaridiaElevatubeVisuals = edited });
            AssertTrue(plms.WreckedShipTreadmill.TrySpawnMaridiaElevatube(level),
                "production door setup spawns the elevatube PLM");
            var guard = NewMaridiaElevatubeSourceGuard(new TestAddressSpace());
            int blockIndex = level.GetBlockIndex(
                new(MaridiaElevatubePlmRomData.BlockX, MaridiaElevatubePlmRomData.BlockY));
            plms.Step(level);
            AssertEqual((ushort)0x8180,
                level.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw,
                "edited elevatube tile retains native physical block");
            // A PLM redraw identifies its block by the BG1 ring destination of that block.
            AssertEqual((ushort)0x0058, level.DisplayedBlockTiles(blockIndex).TopLeft.Raw,
                "edited elevatube reaches immediate tilemap update");
            AssertEqual((ushort)0x0058,
                level.DisplayedBlockTiles(blockIndex).TopLeft.Raw,
                "edited elevatube survives later camera streaming");
            int heardAtFrame = -1;
            for (int frame = 1; frame < 24 && plms.SlotTable.ActiveCount != 0; frame++)
            {
                plms.Step(level);
                if (plms.Sounds.SoundRequests.Any(request =>
                        request.SoundEffect == SoundEffectId.FromCartridge(
                            SoundEffectLibrary.Library2,
                            MaridiaElevatubePlmDefinitionsConstants.SoundId)))
                    heardAtFrame = frame;
            }
            AssertEqual(16, heardAtFrame,
                "edited art leaves native sixteen-frame sound timing intact");
            AssertEqual(0, plms.SlotTable.ActiveCount,
                "edited art leaves native PLM deletion intact");
            AssertEqual(0, guard.ForbiddenReadAttempts,
                "edited elevatube reads no compiled source bytes");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmMaridiaElevatubeVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0058,
                RoomPlmMaridiaElevatubeVisualFiles.Load(refreshed,
                    installation.RoomPlmMaridiaElevatubeVisualOverrideDirectory)
                    .GetWord(MaridiaElevatubePlmDefinitions.DrawPointer, 0, 0),
                "elevatube override survives stock replacement");
            document.Required("entries").Required(0).Required("blocks")[0] = 0xf058;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmMaridiaElevatubeVisuals(),
                "elevatube override cannot edit collision bits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmMaridiaElevatubeVisualFiles.ValidateStock(
                    installation.RoomPlmMaridiaElevatubeVisualDirectory),
                "tampered elevatube stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
        Console.WriteLine(
            "Maridia elevatube visuals: native tile, live edit, physical/sound isolation, stock repair and strict failures pass.");
    }

    /// <summary>Runs the focused elevatube artwork and PLM behavior checks.</summary>
    [VerificationCommand("--maridia-elevatube-visuals")]
    internal static int MaridiaElevatubeVisualsCommand()
    {
        Suite(nameof(VerifyMaridiaElevatubeVisuals), () => VerifyMaridiaElevatubeVisuals());
        return 0;
    }
}
