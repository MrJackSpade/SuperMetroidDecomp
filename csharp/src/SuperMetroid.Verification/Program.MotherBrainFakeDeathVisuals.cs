using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;
using static AssertionsVerification;
using static LookupMotherBrainStockVerification;
using static MotherBrainFakeDeathPlmsVerification;
using static VerificationHelpersVerification;

internal static class MotherBrainFakeDeathVisualsVerification
{
    internal static void VerifyMotherBrainFakeDeathVisuals()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Mother Brain stock oracle revision");
        using var testRootScratch = new TestScratchDirectory("mother-brain-fake-death-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmMotherBrainFakeDeathVisualFiles.Extract(rom,
                installation.RoomPlmMotherBrainFakeDeathVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmMotherBrainFakeDeathVisualFiles.ValidateStock(
                installation.RoomPlmMotherBrainFakeDeathVisualDirectory);
            RoomPlmMotherBrainFakeDeathVisualCatalog stock =
                installation.LoadRoomPlmMotherBrainFakeDeathVisuals();
            VerifyMotherBrainStockMapping(rom, stock);

            string stockPath = Path.Combine(
                installation.RoomPlmMotherBrainFakeDeathVisualDirectory,
                RoomPlmMotherBrainFakeDeathVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException(
                    "Extracted Mother Brain fake-death JSON is empty.");
            Entry("background-row-d")["blocks"]![12] = 0x0058;
            Entry("clear-bottom-right-tube")["blocks"]![5] = 0x0059;
            Directory.CreateDirectory(
                installation.RoomPlmMotherBrainFakeDeathVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmMotherBrainFakeDeathVisualOverrideDirectory,
                RoomPlmMotherBrainFakeDeathVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            RoomPlmMotherBrainFakeDeathVisualCatalog edited =
                installation.LoadRoomPlmMotherBrainFakeDeathVisuals();
            AssertEqual((ushort)0x0058,
                edited.GetWord((ushort)MotherBrainFakeDeathDraw.BackgroundRowD,
                    0, 12),
                "Mother Brain override edits far background tile");
            AssertEqual((ushort)0x0059,
                edited.GetWord(
                    (ushort)MotherBrainFakeDeathDraw.ClearBottomRightTube,
                    1, 0),
                "Mother Brain override edits second-run tube tile");
            VerifyMotherBrainVisualDraw(PlmHeaderId.MotherBrainsBackgroundRowD,
                new(12, 0), 0x1249, 0x0058, edited, cameraX: 64);
            VerifyMotherBrainVisualDraw(PlmHeaderId.ClearMotherBrainBottomRightTube,
                new(-1, 0), 0x00ff, 0x0059, edited, cameraX: 0);
            VerifyMotherBrainVisualDraw(PlmHeaderId.MotherBrainsBackgroundRowD,
                new(12, 0), 0x1249, new RoomLevelWord(0x1249).VisualWord,
                null, cameraX: 64);

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmMotherBrainFakeDeathVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0059,
                RoomPlmMotherBrainFakeDeathVisualFiles.Load(refreshed,
                    installation.RoomPlmMotherBrainFakeDeathVisualOverrideDirectory)
                    .GetWord(
                        (ushort)MotherBrainFakeDeathDraw.ClearBottomRightTube,
                        1, 0),
                "Mother Brain override survives stock replacement");

            Entry("background-row-d")["blocks"]![12] = 0xf058;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmMotherBrainFakeDeathVisuals(),
                "Mother Brain override cannot change collision bits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmMotherBrainFakeDeathVisualFiles.ValidateStock(
                    installation.RoomPlmMotherBrainFakeDeathVisualDirectory),
                "tampered Mother Brain fake-death stock fails manifest validation");

            JsonNode Entry(string id) => document["entries"]!.AsArray()
                .Single(entry => entry!["id"]!.GetValue<string>() == id)!;
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
        Console.WriteLine(
            "Mother Brain fake-death visuals: 22 stock draws, live row/tube edits, collision isolation, override persistence and strict failures pass.");
    }

    private static void VerifyMotherBrainVisualDraw(PlmHeaderId header,
        Offset16<RoomBlocks> blockOffset, ushort physicalWord, ushort visualWord,
        RoomPlmMotherBrainFakeDeathVisualCatalog? visuals, ushort cameraX)
    {
        const int width = 32;
        ushort[] words = Enumerable.Repeat((ushort)0x8123, width * 16).ToArray();
        RoomLevelData level = CreateRoom(new(width, 16), words,
            new byte[words.Length], blockDefinitions: new byte[0x400 * 8]);
        level.SetBlockDefinitionWord(0x0058 * 4, 0x0058);
        level.SetBlockDefinitionWord(0x0059 * 4, 0x0059);
        level.SetBlockDefinitionWord(0x0249 * 4, 0x0249);
        var plms = new RoomPlmSystem { MotherBrainFakeDeathVisuals = visuals };
        AssertTrue(plms.TrySpawnMotherBrainMutation(level, new(5, 3), header),
            $"Mother Brain visual header ${(int)header:X4} allocates");
        var guard = NewMotherBrainFakeDeathSourceGuard(new TestAddressSpace());
        plms.Step(guard, level, level.CreateBackgroundStreamer(), new PlmView { Layer1Position = new(cameraX, 0), Bg1XOffset = 0 });
        int blockIndex = (3 + blockOffset.Y) * width + 5 + blockOffset.X;
        AssertEqual(physicalWord,
            level.GetCollisionBlockByIndex(blockIndex).LevelWord,
            $"Mother Brain visual header ${(int)header:X4} retains physical block");
        // The redraw identifies its block by the block's DrawPLM BG1 ring destination.
        ushort blockDestination = level.CreateBackgroundStreamer()
            .BuildPlmLevelBlockUpdate(blockIndex, 0).TopRowDestination;
        AssertTrue(plms.TilemapUpdates.Any(update =>
                update.TopRowDestination == blockDestination &&
                update.TopRow[0] == visualWord),
            $"Mother Brain visual header ${(int)header:X4} presents edited tile");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Mother Brain visual header ${(int)header:X4} reads no compiled source");
    }

    [VerificationCommand("--mother-brain-fake-death-visuals")]
    internal static int MotherBrainFakeDeathVisualsCommand()
    {
        Suite(nameof(VerifyMotherBrainFakeDeathVisuals), () => VerifyMotherBrainFakeDeathVisuals());
        return 0;
    }
}
