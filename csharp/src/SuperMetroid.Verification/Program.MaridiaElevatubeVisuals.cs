using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;
using static AssertionsVerification;
using static LookupElevatubeStockVerification;
using static MaridiaElevatubePlmVerification;
using static VerificationHelpersVerification;

/// <summary>Represents maridia elevatube visuals verification.</summary>
internal static class MaridiaElevatubeVisualsVerification
{
    /// <summary>Verifies maridia elevatube visuals.</summary>
    internal static void VerifyMaridiaElevatubeVisuals()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
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
            document["entries"]![0]!["blocks"]![0] = 0x0058;
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
            var plms = new RoomPlmSystem { Drawing = { MaridiaElevatubeVisuals = edited } };
            AssertTrue(plms.WreckedShipTreadmill.TrySpawnMaridiaElevatube(level),
                "production door setup spawns the elevatube PLM");
            var guard = NewMaridiaElevatubeSourceGuard(new TestAddressSpace());
            BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
            int blockIndex = level.GetBlockIndex(
                new(MaridiaElevatubePlmRomData.BlockX, MaridiaElevatubePlmRomData.BlockY));
            plms.Step(guard, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
            AssertEqual((ushort)0x8180,
                level.GetCollisionBlockByIndex(blockIndex).LevelWord,
                "edited elevatube tile retains native physical block");
            // A PLM redraw identifies its block by the BG1 ring destination of that block.
            ushort blockDestination = level.CreateBackgroundStreamer()
                .BuildPlmLevelBlockUpdate(blockIndex, 0).TopRowDestination;
            AssertTrue(plms.TilemapUpdates.Any(update =>
                    update.TopRowDestination == blockDestination && update.TopRow[0] == 0x0058),
                "edited elevatube reaches immediate tilemap update");
            AssertEqual((ushort)0x0058,
                level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(
                    blockIndex, 0).TopRow[0],
                "edited elevatube survives later camera streaming");
            int heardAtFrame = -1;
            for (int frame = 1; frame < 24 && plms.SlotTable.ActiveCount != 0; frame++)
            {
                plms.Step(guard, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
                if (plms.Sounds.SoundRequests.Any(request =>
                        request.SoundEffect == SoundEffectId.FromCartridge(
                            SoundEffectLibrary.Library2,
                            MaridiaElevatubePlmDefinitions.SoundId)))
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
            document["entries"]![0]!["blocks"]![0] = 0xf058;
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

    /// <summary>Maridia elevatube visuals command.</summary>
    [VerificationCommand("--maridia-elevatube-visuals")]
    internal static int MaridiaElevatubeVisualsCommand()
    {
        Suite(nameof(VerifyMaridiaElevatubeVisuals), () => VerifyMaridiaElevatubeVisuals());
        return 0;
    }
}
