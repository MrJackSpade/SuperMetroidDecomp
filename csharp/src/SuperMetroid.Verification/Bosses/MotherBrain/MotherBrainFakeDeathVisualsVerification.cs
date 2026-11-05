using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using static SuperMetroid.Verification.Bosses.MotherBrain.LookupMotherBrainStockVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainFakeDeathPlmsVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Checks Mother Brain's installed fake-death artwork and her native collision and draw state.</summary>
internal static class MotherBrainFakeDeathVisualsVerification
{
    /// <summary>Checks the stock mapping, a compiled background row's physical block and tile, and that a malformed installed visual file fails loudly where it is read.</summary>
    internal static void VerifyMotherBrainFakeDeathVisuals()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Mother Brain stock oracle revision");
        // A private installation copy, whose stock file is tampered below.
        using (PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomPlmMotherBrainFakeDeathVisualDirectory))
        {
            GameInstallation installation = copy.Installation;
            RoomPlmMotherBrainFakeDeathVisualFilesConstants.ValidateStock(
                installation.RoomPlmMotherBrainFakeDeathVisualDirectory);
            RoomPlmMotherBrainFakeDeathVisualCatalog stock =
                installation.LoadRoomPlmMotherBrainFakeDeathVisuals();
            VerifyMotherBrainStockMapping(rom, stock);
            VerifyMotherBrainVisualDraw(PlmHeaderId.MotherBrainsBackgroundRowD,
                new(12, 0), RoomTerrainBlock.Of(0x249, BlockMirroring.None, RoomCollisionType.Slope), RoomTerrainBlock.Of(0x249, BlockMirroring.None, RoomCollisionType.Slope).Appearance,
                null);

            string stockPath = Path.Combine(
                installation.RoomPlmMotherBrainFakeDeathVisualDirectory,
                RoomPlmMotherBrainFakeDeathVisualFiles.VisualFileName);
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmMotherBrainFakeDeathVisualFilesConstants.ValidateStock(
                    installation.RoomPlmMotherBrainFakeDeathVisualDirectory),
                "tampered Mother Brain fake-death stock fails loudly where it is read");
        }
        Console.WriteLine(
            "Mother Brain fake-death visuals: 22 stock draws, the compiled background row and strict failures pass.");
    }

    /// <summary>Checks a selected mutation's physical block and redrawn visual tile with source reads guarded.</summary>
    private static void VerifyMotherBrainVisualDraw(PlmHeaderId header,
        Offset16<RoomBlocks> blockOffset, RoomTerrainBlock physical, RoomBlockAppearance visual,
        RoomPlmMotherBrainFakeDeathVisualCatalog? visuals)
    {
        const int width = 32;
        RoomTerrainBlock[] terrain = Enumerable.Repeat(RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock), width * 16).ToArray();
        RoomLevelData level = CreateRoom(new(width, 16), terrain, new RoomBlockBehavior[terrain.Length]);
        var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmMotherBrainFakeDeathVisuals = visuals });
        AssertTrue(plms.BossRoomMutations.TrySpawnMotherBrainMutation(level, new(5, 3), header),
            $"Mother Brain visual header {header} allocates");
        var guard = NewMotherBrainFakeDeathSourceGuard(new TestAddressSpace());
        plms.Step(level);
        int blockIndex = (3 + blockOffset.Y) * width + 5 + blockOffset.X;
        AssertEqual(physical,
            level.GetCollisionBlockByIndex(blockIndex).Terrain,
            $"Mother Brain visual header {header} retains physical block");
        // The redraw identifies its block by the block's DrawPLM BG1 ring destination.
        AssertEqual(visual, level.DisplayedAppearance(blockIndex),
            $"Mother Brain visual header {header} presents its tile");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Mother Brain visual header {header} reads no compiled source");
    }

    /// <summary>Runs the Mother Brain fake-death visual asset suite.</summary>
    [VerificationCommand("--mother-brain-fake-death-visuals")]
    internal static int MotherBrainFakeDeathVisualsCommand()
    {
        Suite(nameof(VerifyMotherBrainFakeDeathVisuals), () => VerifyMotherBrainFakeDeathVisuals());
        return 0;
    }

}
