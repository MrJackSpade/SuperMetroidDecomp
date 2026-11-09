using SuperMetroid.Core.Bosses.MotherBrain;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.LookupMotherBrainStockVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainFakeDeathObjectsVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
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
        using (PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectMotherBrainFakeDeathVisualDirectory))
        {
            GameInstallation installation = copy.Installation;
            RoomObjectMotherBrainFakeDeathVisualFilesConstants.ValidateStock(
                installation.RoomObjectMotherBrainFakeDeathVisualDirectory);
            RoomObjectMotherBrainFakeDeathVisualCatalog stock =
                installation.LoadRoomObjectMotherBrainFakeDeathVisuals();
            VerifyMotherBrainStockMapping(rom, stock);
            VerifyMotherBrainVisualDraw(MotherBrainRoomMutation.BackgroundRow13,
                new(12, 0), RoomTerrainBlock.Of(0x249, BlockMirroring.None, RoomCollisionType.Slope), RoomTerrainBlock.Of(0x249, BlockMirroring.None, RoomCollisionType.Slope).Appearance,
                null);

            string stockPath = Path.Combine(
                installation.RoomObjectMotherBrainFakeDeathVisualDirectory,
                RoomObjectMotherBrainFakeDeathVisualFiles.VisualFileName);
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomObjectMotherBrainFakeDeathVisualFilesConstants.ValidateStock(
                    installation.RoomObjectMotherBrainFakeDeathVisualDirectory),
                "tampered Mother Brain fake-death stock fails loudly where it is read");
        }
        Console.WriteLine(
            "Mother Brain fake-death visuals: 22 stock draws, the compiled background row and strict failures pass.");
    }

    /// <summary>Checks a selected mutation's physical block and redrawn visual tile with source reads guarded.</summary>
    private static void VerifyMotherBrainVisualDraw(MotherBrainRoomMutation mutation,
        Offset16<RoomBlocks> blockOffset, RoomTerrainBlock physical, RoomBlockAppearance visual,
        RoomObjectMotherBrainFakeDeathVisualCatalog? visuals)
    {
        const int width = 32;
        RoomTerrainBlock[] terrain = Enumerable.Repeat(RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock), width * 16).ToArray();
        RoomLevelData level = CreateRoom(new(width, 16), terrain, new RoomBlockBehavior[terrain.Length]);
        var roomObjects = RoomObjectFixtures.Unbound(new RoomObjectVisuals { RoomObjectMotherBrainFakeDeathVisuals = visuals });
        AssertTrue(roomObjects.BossRoomMutations.MotherBrainRoom.TrySpawn(level, new(new(5, 3), mutation)),
            $"Mother Brain visual mutation {mutation} allocates");
        var guard = NewMotherBrainFakeDeathSourceGuard(new TestAddressSpace());
        roomObjects.Step(level);
        int blockIndex = (3 + blockOffset.Y) * width + 5 + blockOffset.X;
        AssertEqual(physical,
            level.GetCollisionBlockByIndex(blockIndex).Terrain,
            $"Mother Brain visual mutation {mutation} retains physical block");
        // The redraw identifies its block by the block's DrawPLM BG1 ring destination.
        AssertEqual(visual, level.DisplayedAppearance(blockIndex),
            $"Mother Brain visual mutation {mutation} presents its tile");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Mother Brain visual mutation {mutation} reads no compiled source");
    }

    /// <summary>Runs the Mother Brain fake-death visual asset suite.</summary>
    [VerificationCommand("--mother-brain-fake-death-visuals")]
    internal static int MotherBrainFakeDeathVisualsCommand()
    {
        Suite(nameof(VerifyMotherBrainFakeDeathVisuals), () => VerifyMotherBrainFakeDeathVisuals());
        return 0;
    }

}
