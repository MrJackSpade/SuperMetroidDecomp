using SuperMetroid.Verification.Rooms.Plms;
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
            document.Required("entries").Required(0).Required("blocks")[0] = PlmVisualJson.Appearance(0x058);
            Directory.CreateDirectory(
                installation.RoomPlmMaridiaElevatubeVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmMaridiaElevatubeVisualOverrideDirectory,
                RoomPlmMaridiaElevatubeVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            InstalledRoomPlmMaridiaElevatubeVisuals edited =
                installation.LoadRoomPlmMaridiaElevatubeVisuals();
            AssertEqual(RoomBlockAppearance.Of(0x058),
                edited.GetAppearance(MaridiaElevatubePlmDefinitions.BlockDraw, 0, 0),
                "elevatube override replaces only the visible block");

            RoomLevelData level = CreateRoom(new(4, 4), new RoomTerrainBlock[16], new RoomBlockBehavior[16]);
            var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmMaridiaElevatubeVisuals = edited });
            AssertTrue(plms.WreckedShipTreadmill.TrySpawnMaridiaElevatube(level),
                "production door setup spawns the elevatube PLM");
            var guard = NewMaridiaElevatubeSourceGuard(new TestAddressSpace());
            int blockIndex = level.GetBlockIndex(MaridiaElevatubeDefinitions.PlmBlock);
            plms.Step(level);
            AssertEqual(RoomTerrainBlock.Of(0x180, BlockMirroring.None, RoomCollisionType.SolidBlock),
                level.GetCollisionBlockByIndex(blockIndex).Terrain,
                "edited elevatube tile retains native physical block");
            // A PLM redraw identifies its block by the BG1 ring destination of that block.
            AssertEqual(RoomBlockAppearance.Of(0x058), level.DisplayedAppearance(blockIndex),
                "edited elevatube reaches immediate tilemap update");
            AssertEqual(RoomBlockAppearance.Of(0x058),
                level.DisplayedAppearance(blockIndex),
                "edited elevatube survives later camera streaming");
            int heardAtFrame = -1;
            for (int frame = 1; frame < 24 && plms.SlotTable.ActiveCount != 0; frame++)
            {
                plms.Step(level);
                if (plms.Sounds.SoundRequests.Any(request =>
                        request.SoundEffect == MaridiaElevatubePlmDefinitionsConstants.SoundId))
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
            AssertEqual(RoomBlockAppearance.Of(0x058),
                RoomPlmMaridiaElevatubeVisualFiles.Load(refreshed,
                    installation.RoomPlmMaridiaElevatubeVisualOverrideDirectory)
                    .GetAppearance(MaridiaElevatubePlmDefinitions.BlockDraw, 0, 0),
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
