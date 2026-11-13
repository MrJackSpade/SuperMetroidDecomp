using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
using SuperMetroid.Core.Bosses.Kraid;
using static SuperMetroid.Verification.Bosses.Kraid.KraidRoomObjectsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidRoomVisualSelectionVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Kraid ceiling and spike PLM visuals and their block streaming.</summary>
internal static class KraidRoomVisualsVerification
{
    /// <summary>Checks the stock draws, the compiled first crumble's physical block and tile, and that a malformed installed visual file fails loudly where it is read.</summary>
    internal static void VerifyKraidRoomVisuals()
    {
        Suite(nameof(VerifyKraidRoomVisualSelection), () => VerifyKraidRoomVisualSelection());
        CartridgeImportAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        // A private installation copy, whose stock file is tampered below.
        using (PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectKraidVisualDirectory))
        {
            GameInstallation installation = copy.Installation;
            RoomObjectKraidVisualFilesConstants.ValidateStock(
                installation.RoomObjectKraidVisualDirectory);
            InstalledRoomObjectKraidVisuals stock =
                installation.LoadRoomObjectKraidVisuals();
            LookupEscapeGateStockFrame[] frames = [
                new LookupEscapeGateStockFrame(0x9367, "crumble-first"), new LookupEscapeGateStockFrame(0x936d, "crumble-second"),
                new LookupEscapeGateStockFrame(0x9373, "crumble-third"), new LookupEscapeGateStockFrame(0x9379, "ceiling-background-one"),
                new LookupEscapeGateStockFrame(0x937f, "ceiling-background-two"), new LookupEscapeGateStockFrame(0x9385, "ceiling-background-three"),
                new LookupEscapeGateStockFrame(0x9391, "spike-first"), new LookupEscapeGateStockFrame(0x9397, "spike-second"),
                new LookupEscapeGateStockFrame(0x939d, "clear-ceiling"), new LookupEscapeGateStockFrame(0x93bf, "clear-spikes")];
            AssertPicturedAsNative(rom, stock.Images, RoomObjectKraidVisualFiles.Family, frames, WrittenCells(RoomObjectKraidVisualFiles.Family));
            AssertImportAndRejections(rom, stock.Images, RoomObjectKraidVisualFiles.Family, installation.RoomObjectKraidVisualDirectory);
            Suite(nameof(VerifyKraidDraw), () => VerifyKraidDraw(KraidRoomMutation.CrumbleCeilingIntoBackground1, RoomObjectDrawAddressCodec.Of(RoomObjectDraw.Of(KraidRoomDraw.CrumbleFirst)), 0, RoomTerrainBlock.Of(0x180, BlockMirroring.None, RoomCollisionType.SolidBlock), RoomBlockAppearance.Of(0x180)));

            string stockPath = Path.Combine(
                installation.RoomObjectKraidVisualDirectory,
                RoomObjectKraidVisualFiles.VisualFileName);
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomObjectKraidVisualFilesConstants.ValidateStock(
                    installation.RoomObjectKraidVisualDirectory),
                "tampered Kraid stock fails loudly where it is read");
        }
        Console.WriteLine(
            "Kraid room visuals: ten native draws, the compiled first crumble and strict failures pass.");
    }

    /// <summary>Spawns a Kraid mutation and checks its collision word and streamed tile.</summary>
    private static void VerifyKraidDraw(KraidRoomMutation mutation, ushort drawPointer, int blockOffset, RoomTerrainBlock physicalWord, RoomBlockAppearance visualWord)
    {
        const int width = 32;
        var words = new RoomTerrainBlock[width * 16];
        for (int x = 5; x <= 26; x++)
            words[5 * width + x] = RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock);
        RoomLevelData level = CreateRoom(new(width, 16), words,
            new RoomBlockBehavior[words.Length]);
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertTrue(roomObjects.BossRoomMutations.KraidRoom.TrySpawn(level, new(new(5, 5), mutation)),
            $"Kraid visual owner {mutation} allocates");
        var guard = NewKraidRoomSourceGuard(new TestAddressSpace());
        roomObjects.Step(level);
        Position16<RoomBlocks> block = new(5 + blockOffset, 5);
        int blockIndex = level.GetBlockIndex(block);
        AssertEqual(physicalWord,
            level.GetCollisionBlock(block).Terrain,
            $"Kraid draw ${drawPointer:X4} retains physical block {blockOffset}");
        AssertEqual(visualWord,
            level.DisplayedAppearance(blockIndex),
            $"Kraid draw ${drawPointer:X4} tile survives streaming");
        AssertEqual(visualWord, level.DisplayedAppearance(blockIndex),
            $"Kraid draw ${drawPointer:X4} uses owner-specific visible block {blockOffset}");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Kraid draw ${drawPointer:X4} reads no migrated source bytes");
    }

    /// <summary>Runs the Kraid room visual installation and ownership verification command.</summary>
    [VerificationCommand("--kraid-room-visuals")]
    internal static int KraidRoomVisualsCommand()
    {
        Suite(nameof(VerifyKraidRoomVisuals), () => VerifyKraidRoomVisuals());
        return 0;
    }
}
