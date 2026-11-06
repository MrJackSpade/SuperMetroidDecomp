using SuperMetroid.Core.Bosses.Kraid;
using static SuperMetroid.Verification.Bosses.Kraid.KraidRoomPlmsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidRoomVisualSelectionVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
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
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        // A private installation copy, whose stock file is tampered below.
        using (PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomPlmKraidVisualDirectory))
        {
            GameInstallation installation = copy.Installation;
            RoomPlmKraidVisualFilesConstants.ValidateStock(
                installation.RoomPlmKraidVisualDirectory);
            InstalledRoomPlmKraidVisuals stock =
                installation.LoadRoomPlmKraidVisuals();
            foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in
                     RoomPlmKraidVisualFiles.All)
            {
                for (int block = 0; block < draw.Runs.Span[0].Count;
                     block++)
                    AssertEqual(draw.Runs.Span[0].Blocks.Span[block].Appearance,
                        stock.GetAppearance(draw.Draw, 0, block),
                        $"Kraid stock draw ${PlmDrawAddressCodec.Of(draw.Draw):X4}, block {block}");
            }
            AssertThrows<InvalidDataException>(
                () => RoomPlmKraidVisualFiles.Create([]),
                "Kraid visual catalog rejects missing frames");
            Suite(nameof(VerifyKraidDraw), () => VerifyKraidDraw(KraidRoomMutation.CrumbleCeilingIntoBackground1, PlmDrawAddressCodec.Of(PlmDraw.Of(KraidRoomDraw.CrumbleFirst)), 0, RoomTerrainBlock.Of(0x180, BlockMirroring.None, RoomCollisionType.SolidBlock), RoomBlockAppearance.Of(0x180)));

            string stockPath = Path.Combine(
                installation.RoomPlmKraidVisualDirectory,
                RoomPlmKraidVisualFiles.VisualFileName);
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmKraidVisualFilesConstants.ValidateStock(
                    installation.RoomPlmKraidVisualDirectory),
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
        var plms = PlmFixtures.Unbound();
        AssertTrue(plms.BossRoomMutations.KraidRoom.TrySpawn(level, new(new(5, 5), mutation)),
            $"Kraid visual owner {mutation} allocates");
        var guard = NewKraidRoomSourceGuard(new TestAddressSpace());
        plms.Step(level);
        int blockIndex = 5 * width + 5 + blockOffset;
        AssertEqual(physicalWord,
            level.GetCollisionBlockByIndex(blockIndex).Terrain,
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
